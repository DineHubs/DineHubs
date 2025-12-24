import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { Subscription } from 'rxjs';
import { HeaderComponent } from '../header/header.component';
import { SidebarComponent } from '../sidebar/sidebar.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    HeaderComponent,
    SidebarComponent
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss'
})
export class MainLayoutComponent implements OnInit, OnDestroy {
  private breakpointObserver = inject(BreakpointObserver);
  private breakpointSubscription?: Subscription;
  private isInitialized = false;
  
  // Mobile/tablet: < 1024px (lg breakpoint)
  isMobile = signal(false);
  sidenavOpened = signal(true);

  ngOnInit(): void {
    // Use custom breakpoint at 1024px (Tailwind lg) for better desktop detection
    this.breakpointSubscription = this.breakpointObserver
      .observe(['(max-width: 1023px)'])
      .subscribe(result => {
        const wasMobile = this.isMobile();
        this.isMobile.set(result.matches);
        
        // Only set initial state on first load, or when switching between mobile/desktop
        if (!this.isInitialized) {
          this.isInitialized = true;
          // Start with sidebar open on desktop, closed on mobile
          this.sidenavOpened.set(!result.matches);
        } else if (wasMobile !== result.matches) {
          // When switching between mobile and desktop, auto-close on mobile
          if (result.matches) {
            this.sidenavOpened.set(false);
          }
          // Don't force open on desktop - let user control it
        }
      });
  }

  ngOnDestroy(): void {
    this.breakpointSubscription?.unsubscribe();
  }

  toggleSidenav(): void {
    this.sidenavOpened.set(!this.sidenavOpened());
  }
}
