import {Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import * as signalR from '@microsoft/signalr';
import {Field} from "./core/model/field.model";
import {FieldData} from "./core/model/field-data.model";
import {Player} from "./core/model/player.model";
import {PlayerData} from "./core/model/player-data.dummy";
import {_closeDialogVia} from "@angular/material/dialog";
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  username!: string;
  private _connection!: signalR.HubConnection;

  players: Player[] = [];
  text: string = "";
  fields: Field[] = FieldData;
  isGameReady = false;
  isPlayerNameSubmited = false;
  currentPlayer!: Player;
  isBlocked: boolean = false;

  send(){
    const username =this.username;
    this.isPlayerNameSubmited = true;
    this._connection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:44332/tictactoehub?username=' + username)
      .configureLogging(signalR.LogLevel.Trace)
      .build();

    this._connection.start().then(function () {
    }).catch(function (err:any) {
      console.error(err.toString());
    });

    this._connection.on("AllPlayerReady",  (players: Player[]) => {
      this.players = players;
      this.isGameReady = true;
      this.currentPlayer = players.find(p => p.currentTurn)!;
      const id = this.currentPlayer.id.toString();
      if(this._connection.connectionId !== id){
        this.isBlocked = true;
      }else {
        this.isBlocked = false;
      }
    })

    this._connection.on("ReciveFieldIdToUpdate", (fields: Field[]) => {
      this.fields = fields;
    })

    this._connection.on("ReciveWinner", (fields: Field[], players: Player[]) => {
      this.fields = fields;
      this.players = players;
      this.isBlocked = true;
      console.log(this.players)
      this.resetGame();
    })

    this._connection.on("ReciveUpdatedPlayers", (players: Player[]) => {
      this.players = players;
      this.currentPlayer = players.find(p => p.currentTurn)!;
      const id = this.currentPlayer.id.toString();
      if(this._connection.connectionId !== id){
        this.isBlocked = true;
      }else {
        this.isBlocked = false;
      }
        console.log(this.players)
    })

    this._connection.on("DrawGame",() => {
      this._connection.invoke("UpdatePlayers");
      this.resetGame();
    })
  }

  onField(field: Field){
    if(!!field.sign){
      return
    }else {
      this._connection.invoke("UpdateField", field.id);
    }
  }
  resetGame(){
    setTimeout( () => {
      this.fields.forEach(x => {
        x.win = false;
        x.sign = '';
      })
      this.isBlocked = false;
    }, 1000);
  }
}
