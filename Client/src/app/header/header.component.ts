import {Component, Input, OnInit} from '@angular/core';
import {Player} from "../core/model/player.model";

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent{
  @Input() players!: Player[];
}
