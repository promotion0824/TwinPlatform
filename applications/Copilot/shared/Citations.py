from typing import List, Literal
import re
from itertools import count, groupby
from collections import OrderedDict

from shared.OpenTelemetry import log_warning

CitationsMode = Literal['inline', 'refs',  'refs-pages', 'off']

# [[title:'foo.pdf', page:23]] or [[title:'foo.pdf']]
# also handle these cases which the llm might generate:
# [[foo.pdf, page 23]]

# See Prompts.py MainPrompt for instructions to the LLM for formatting citations
#  - as it doesn't always get it right, we try and be flexible here

# Allow page to be any string - allow for LLM halucinations and non-numeric page refs like "Sec. 1.3"
# Use non-greedy/lazy matching +? of strings in quotes to avoid over-matching (could also use [^']+)
# TODO: If the LLM has halucinated "name" instead of title - could allow anything before the colon
regex_citation = r"\[\[(?:title:)?'?(?P<title>.+?)'?(?:,? ?pages?:? ?(?P<page>.+?))?\]\]"
regex_catchall = r"\[\[.+?\]\]"

# TODO: ref-with-page mode

def get_citations(
                text, 
                citations_mode: CitationsMode = 'refs-pages',
                append_citations_text: bool = False,
                ) -> tuple[str, list[tuple[str, list[str]]]]: 

    """
    Collect citations from the text for reponse.Citations and optionally replace
     them the text them using the following citations_mode:
    - inline: Leave citations in the text as is from the response: [[...]]
    - refs: Replace citations in the text with a reference number 
          that corresponds to the index of the file in the Citations list: <<<1>>>
    - refs-pages: Same as above, but also include the page number(s) in the ref: <<<1:13>>>
    - off: Remove citations from the text entirely - substitute with empty string
           Citations with pages list are still extracted for the response.
    If inline citations are used, it is expected that the UI will reformat these appropriately.
    Note that multiple pages are also supposed for a single citation: <<<1: 13, 27, 42>>>
    We could also look at ParsePath instead of PageNumber - these are the same at the moment
      until we have semantic parising that is heading/section/etc aware.
    """

    citations = OrderedDict()
    orig_text = text

    i = 0
    # Build citations dictionary and replace citations in text with
    #   ref placeholders or the empty string, depending on mode
    while i < 100:
        m = re.search(regex_citation, text)
        if not m: break
        file, pages = m.group('title'), m.group('page')

        if file.lower() in ['context', 'history', 'source']:
            # Sometimes the LLM seems to tell us its referencing history or context -
            # (or will make up a "source" when no docs are returned)
            #  this is interesting and we may want to do something with it in the future.
            # But for now we'll remove from the citiations. 
            # This assumes we won't have a file such as "history" but "history.pdf" is fine.
            log_warning(f"Ingoring history/context citation in: {file} [{pages}]")
            text = re.sub(regex_citation, "", text, count = 1)
            continue

        if not file in citations: citations[file] = []
        iref = 1 + list(citations.keys()).index(file)

        if pages:
            if pages == 'unknown': 
                # LMM is supposed to omit in the case, but sometimes returns 'unknown'
                pages = []
                log_warning(f"'unknown' page ref from LLM: {pages}")
            elif pages.isdigit():  # isdigit == allDigits
                # Single page: '23'
                pages = [pages]
            elif ',' in pages:
                # List of pages: 1, 23, 42
                pages = [p.strip() for p in pages.split(",")]
            else:
                # Something else
                # could return [] here, but consistent with not checking for digits in comma seperated case
                # This allows for non-numeric pages like "Section 1.3" if we store in index
                pages = [page] 
                log_warning(f"returning unknown page ref format from LLM: {pages}")

            for page in pages:
                if page: citations[file].append( page)

        if citations_mode == 'refs' or (citations_mode == 'refs-pages' and not pages): 
            sub = f"<<<{iref}>>>"
        elif citations_mode == 'refs-pages' and pages: 
            sub = f"<<<{iref}: {', '.join(pages)}>>>"
        else: 
            sub = ""
        text = re.sub(regex_citation, sub, text, count = 1)
        i += 1

    # Ensure proper numeric order with lexical sort, but allow for non-numeric page refs from model like "Sec. 1.3" or bad citation parses
    def sorter(x): return f"{x:03}" if x.isdigit() else x
    # convert dict of lists to list of tuples with sorted, unique pages
    citations_list = [
        (file, sorted( set(refs), key = sorter)) 
            for file, refs in citations.items()
    ]
    # "text", [('foo.pdf', ['13', '27']), ('bar.pdf', ['99']), ('baz.doc', [])]

    use_markdown = True
    def mdi(s): return f"{s}. " if use_markdown else f"[{i}]: "
    def mdp(s): return f"**{s}**" if use_markdown else s
    def mdf(s): return f"*{s}*" if use_markdown else s

    if append_citations_text and citations_list:
        if use_markdown: text += "\n\n---\n"
        else: text += "\n\nCitations:\n\n"
        for i, (file, refs_list) in enumerate(citations_list):
            text += f"{mdi(1+i)} {mdf(file)}"
            if len(refs_list) == 0:
                text += "\n"
            else:
                refs = f"p. {mdp(refs_list[0])}" if len(refs_list)==1 else \
                       f"pp. {', '.join([mdp(r) for r in refs_list])}"
                text += f" ({refs})\n"
        if use_markdown:
            text += "```\n"

    # remove anything else inside of [[...]] that we may not have handled
    #  - won't cover wider halucinations that are outside of this format
    text = re.sub(regex_catchall, "", text)

    return_text = text.strip() if citations_mode != 'inline' else orig_text
    return return_text, citations_list

if __name__ == "__main__":
    """Text extraction of citations and reference-replacement"""

    text = """
    Once upon a time
    This is a test [[title:'foo.pdf', page:27]], with text after
    This is more [[title:'foo.pdf', page:13]], with text after
    from ans [[title:'3086855_MPCETNM6S_SC_IO_EN Hussman ref case installation and operations manual.pdf', page:9]]
    and one without a page [[title:'foo.pdf']] with more text.
    [[title:'bar.pdf', page:99]]. sdf sdfksld fslkdf 
    [[title:'bar.pdf', page:99]]. and
    blah blah [[title:'baz.doc']].
    the first file again [[title:'foo.pdf', page:27]].
    and the wrong format which we need to handle  [[BAD.pdf, page 42]]
    and [[BAD.pdf]] and just in case [[BAD.pdf, 222]]
    and multiple pages [[BADM.pdf, page: 1, 22, 33]]
    This is completly random and should be removed [[name:foo.pdf, on page 43]]
    These are next to each other: [[title:'foo.pdf', page:27]][[title:'foo.pdf', page:13]]
    The context and history is in here and will be removed: [[title:'context']] and [[title:'history']]
    The end
    """

    import pprint
    mode: CitationsMode = 'off'
    c = get_citations(text, mode, append_citations_text=True)
    print(c[0])
    pprint.pprint(c[1])
