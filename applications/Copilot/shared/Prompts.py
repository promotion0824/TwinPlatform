
from typing import List
import re

from langchain.prompts import PromptTemplate

from langchain.chains.conversational_retrieval.prompts import CONDENSE_QUESTION_PROMPT
from langchain.memory.prompt import SUMMARY_PROMPT

def trim_extra_internal_whitespace(s:str) -> str: 
    return re.sub(r'[ \t][ \t]+', ' ', s)

class MainPrompt:
    """
    Return the prompt used to process the main chat request.
    """

    def get_prompt_text(self, prompt_hints: List[str]) -> str:

        prompt_citations = """
            After each sentence or paragraph for any information that you use from the context, 
              add a citation that specifies the source of the information.
            An example citation is: "[[title:'foo.pdf', page:23]]"
            if the page_number is absent or unknown then omit it from the citation,
            for example: "[[title:'foo.pdf']]"
            Do not add any other text around each citation or add a seperate citations list at the end of the response.
            For example, use simply "[[title:'foo.pdf']]" not "Citation: [[title:'foo.pdf']]"
        """

        prompt_preamble = """
            ###Instructions###
            You are a helpful AI system called Copilot designed to help professional technicians 
             solve problems with equipment and perform repairs.
            Ensure that responses adhere to the following rules:
            - General knowledge must be in the domains of:
                 Real Estate, HVAC, Regfrigeration, Electrical, 
                 Plumbing, and related fields only.
            - When possible, answer with itemized and numbered lists
            - If the information is already in a bulleted or numbered list, keep it in the same format and do not rephrase.
            - Do not tell the user to follow safety guidelines or refer to a manual.
            - Do not tell the user to contact a professional, expert, specialist, technical support or any other party.
            - Do not ask the user to visit a website or tell the user may void the warranty.
        """
        prompt_markdown = """
            Use markdown to format your response.
            Any company names, model names, serial or part numbers, etc., should be shown in bold,
            such as "**ACME** model **ABC123**" or "part **XYZ456**".
            Numbers should be shown in bold, such as "**24** volts".
            """

        _ = """If your answer doesn't meet these criteria, simple respond with "I don't know."
            """

        prompt_details = """If you do not have the specific details of equipment to provide detailed information finish your 
            answer by describing the missing details and suggesting that the user can provide the specific details 
            you need and you will attempt to improve the details of your answer."""

        prompt_main = """
            Keep this question in mind: <question> {question} </question>
            <history>\n {chat_history} \n </history>
            <context>\n {context} </context> 
            <question> {question} </question>
            """

        # Reminding the LLM (esp.GPT3) to do this again at the end seems to help
        prompt_addendum = """
            The answer to the question with citations and markdown without advising the user 
            to seek further assistance is:
            Copilot:
        """

        prompt_template_items = [
            prompt_preamble, 
            prompt_markdown,
            prompt_details,
            prompt_citations, 
            prompt_main, 
            prompt_addendum
        ]
        prompt_template_text = ''.join([trim_extra_internal_whitespace(t) for t in prompt_template_items])
        return prompt_template_text


    def get_prompt_template(self, prompt_hints: List[str] | None) -> PromptTemplate:

        prompt_text = self.get_prompt_text(prompt_hints)
        prompt_hints = prompt_hints or []
        prompt_template = PromptTemplate(
            template = prompt_text, 
            input_variables = [ "chat_history", "context", "question"]
        )
        return prompt_template

class DocumentChunkPrompt:
    """
    Return the prompt used to process each index document chunk.
    This is necessary to add metadata such as the filename so that citations can be added.
    Note that rather using using doc metadata fields like Title or PageNumber here,
     we have set a combined _Citation in our CustomVectorStoreRetriever
    """
    VirtualFieldnameCitation = "_Citation"
    VirtualFieldnameMetadata = "_Metadata"
    MissingMetadataValue = "*"

    # Unfortunately, langchain templates aren't as powerful as mustache templates,
    #  so we can't omit surrounding text based on the absence of a field.
    def get_prompt_text(self):
        #return """citation:{_citationField} context: {page_content}\n\n """
        return f"""
            citation: {{{self.VirtualFieldnameCitation}}} \
            extra info: "{{{self.VirtualFieldnameMetadata}}}" 
            <content> \n {{page_content}} </content> \n
        """

    def get_prompt_template(self) -> PromptTemplate:

        prompt_template = PromptTemplate(
            template = self.get_prompt_text().strip(),
            input_variables = [
                "page_content", "Title", 
                self.VirtualFieldnameCitation, self.VirtualFieldnameMetadata
            ],
            #partial_variables = { "_citationField": VirtualFieldnameCitation, "_descriptionField": VirtualFieldnameDescription }
        )

        return prompt_template


class CondenseQuestionPrompt:
    """
    Return the prompt used to rephrase the user's question based on the chat history.

    The langchain default is: 

        Given the following conversation and a follow up question, rephrase the follow up 
            question to be a standalone question, in its original language.
        Chat History:
        {chat_history}
        Follow Up Input: {question}
        Standalone question:
    """

    CustomPromptText = """
            Given the following conversation history and a follow up question, rephrase the follow up 
                question to be a standalone question, in its original language.
            If the follow up question does not need context, return the unmodified question.
            Be sure to extract and maintain all details of the asset under discussion 
             such as the Manufacturer, Model, Serial Number, Part Number, and any other relevant details and specifications.
            ###Example###
            Question: Thank you
            Chat History: ...
            Rephrased Standalone question: Thank you
            ###Example###
            Question: How do I replace the filter?
            Chat History: The ACME model 123 runs in 115V AC
            Rephrased Standalone question: How do I replace the filter on the ACME model 123?
        """

    template_suffix = """
        <history> {chat_history} </history>
        <follow_up_question> {question} <follow_up_question>
        Rephrased Standalone question:"""
    
    def get_default_prompt_template(self) -> PromptTemplate:
        return CONDENSE_QUESTION_PROMPT

    def get_prompt_template(self, prompt_text: str = None) -> PromptTemplate:
        prompt_text = prompt_text or self.CustomPromptText
        return PromptTemplate.from_template(f"{prompt_text}\n{self.template_suffix}")

class MemorySummarizePrompt:
    """
    When the history becomes too long, the oldest interactions are popped off the history buffer.
      until we are below the given history token limit, and then rephreased.
    Return the prompt used to rephrase the user's question based on the chat history.

    The langchain default is: 

        Progressively summarize the lines of conversation provided, adding onto the previous summary returning a new summary.

        EXAMPLE
        Current summary:
        The human asks what the AI thinks of artificial intelligence. The AI thinks artificial intelligence is a force for good.
        New lines of conversation:
        Human: Why do you think artificial intelligence is a force for good?
        AI: Because artificial intelligence will help humans reach their full potential.
        New summary:
        The human asks what the AI thinks of artificial intelligence. The AI thinks artificial intelligence is a force for good because it will help humans reach their full potential.
        END OF EXAMPLE

        Current summary:
        {summary}
        New lines of conversation:
        {new_lines}
        New summary:
        """

    CustomPromptText = """
        Progressively summarize the lines of conversation provided, adding onto the previous summary returning a new summary.
        Be sure to extract and maintain all details conversation including:
            - The Manufacturer, Model, Serial Number, Part Number, and any other relevant details and specifications.
            - The current goal that the user is trying to accomplish.
            - Any steps that the user has taken and still needs to take.
        
        EXAMPLE
        Current summary:
        The user is discussing the ACME model 123 refrigeration unit. The problem is that
        it's running to warm. The AI has recommended the user change the filter.
        The AI recommended that the user (1) turn off the unit, (2) remove the front panel, (3) remove the old filter, and (4) replace with the new filter.

        New lines of conversation:
        Human: I've turned off the unit and done step 2. What should I do next?
        AI: The next steps are to (3) remove the old filter (4) replace it with the new filter.
        Human: I've removed the old filter. How often should I replace it.
        AI: It is recommended to replace the filter every 3 months. [[title:'ACME manual', page: 12]]
        Human: Ok, I've replaced it. What should I do now?
        AI: You are done with the filter replacement. You can now turn the unit back on.
            The final step (5) is to check that the Change Filter LED is now off.

        New summary:
        The user is discussing the ACME model 123 refrigeration unit which is runing warm and
         is in the process of changing the filter.
        The AI recommended that the user (1) turn off the unit, (2) remove the front panel, (3) remove the old filter, and (4) replace with the new filter,
        and (5) restart the unit and make sure the Change Filter LED is now off.  
        The user has completed steps 1, 2, 3, and 4 but still needs to restart the unit and verify that the Change Filter LED is now off.
        END OF EXAMPLE
        """

    template_suffix = """
        Current summary:
        {summary}
        New lines of conversation:
        {new_lines}
        New summary: """
    
    def get_default_prompt_template(self) -> PromptTemplate:
        return SUMMARY_PROMPT

    def get_prompt_template(self, prompt_text: str = None) -> PromptTemplate:
        prompt_text = prompt_text or self.CustomPromptText
        return PromptTemplate.from_template(f"{prompt_text}\n{self.template_suffix}")

class SummarizeDocumentPrompt:
    """
    Get the prompt used to summarize a document.
    TODO: We should load these from BLOB stoage, and possibly optionally allow a different one for 
      each type of document category.
    """
    
    template = """Summarize the paragraphs below.
                    Be sure to include any information about the following topics:
                    Manufacturer, models, serial Numbers, part numbers, and any other relevant details about the asset.
                    However, do not mention if any of this information is missing.
                    Do not mention anything about seeking the advice of a professional,
                        or the document being subject to change.
                    Do not mention anything about copyrights.
                    Refer to "the document" not "text" or "documents".
                    ======================
                    {text}
                    """  

    def get_prompt_template(self) -> PromptTemplate:
        return PromptTemplate.from_template(self.template)

