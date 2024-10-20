export interface ICopilotMessage {
  id: number,
  text: string,
  sender: string | undefined,
  isBot: boolean
}

export class CopilotMessage implements ICopilotMessage {
  id: number;
  text: string;
  sender: string;
  isBot: boolean;
  constructor(message: string, userName: string | undefined, isBotResponse: boolean) {
    this.id = new Date().getTime();
    this.text = message;
    this.sender = isBotResponse ? "Copilot" : (userName || "Unknown User");
    this.isBot = isBotResponse;
  }
}
