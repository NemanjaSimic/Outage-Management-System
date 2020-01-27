export enum SwitchCommandType {
  TURN_OFF = 1,
  TURN_ON = 0
}

export interface SwitchCommand {
  guid: number,
  type: SwitchCommandType
} 
