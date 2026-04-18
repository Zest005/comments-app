import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { I18nService, Lang } from '../../services/i18n.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class HeaderComponent {
  constructor(public i18n: I18nService) {}

  setLang(lang: Lang): void {
    this.i18n.setLang(lang);
  }
}
