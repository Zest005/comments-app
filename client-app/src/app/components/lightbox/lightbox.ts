import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { LightboxService } from '../../services/lightbox.service';

@Component({
  selector: 'app-lightbox',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './lightbox.html',
  styleUrls: ['./lightbox.css']
})
export class LightboxComponent implements OnInit, OnDestroy {
  showLightbox = false;
  closingLightbox = false;
  lightboxUrl = '';

  private sub!: Subscription;

  constructor(private lightboxService: LightboxService) {}

  ngOnInit(): void {
    this.sub = this.lightboxService.open$.subscribe(url => {
      this.lightboxUrl = url;
      this.showLightbox = true;
      this.closingLightbox = false;
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  close(): void {
    this.closingLightbox = true;
    setTimeout(() => {
      this.showLightbox = false;
      this.closingLightbox = false;
      this.lightboxUrl = '';
    }, 300);
  }
}