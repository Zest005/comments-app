import { Component } from '@angular/core';
import { HeaderComponent } from './components/header/header';
import { CommentListComponent } from './components/comment-list/comment-list';
import { LightboxComponent } from './components/lightbox/lightbox';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [HeaderComponent, CommentListComponent, LightboxComponent],
  template: `
    <app-header></app-header>
    <app-comment-list></app-comment-list>
    <app-lightbox></app-lightbox>
  `,
  styles: [`
    :host {
      display: block;
      min-height: 100vh;
      background: #f5f5f5;
    }
  `]
})
export class AppComponent {}