export interface Player {
  id: number,
  wins: number,
  sign: 'x' | 'o',
  name: string,
  currentTurn: boolean
}
