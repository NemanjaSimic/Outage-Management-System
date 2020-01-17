export enum SwitchCommandType {
  TURN_OFF = 0,
  TURN_ON = 1
}

export interface SwitchCommand {
  guid: number,
  type: SwitchCommandType
} 
