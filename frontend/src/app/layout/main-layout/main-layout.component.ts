import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
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
export class MainLayoutComponent implements OnInit {
  private breakpointObserver = inject(BreakpointObserver);
  
  isMobile = signal(false);
  sidenavOpened = signal(true);

  ngOnInit(): void {
    this.breakpointObserver.observe([Breakpoints.Handset]).subscribe(result => {
      this.isMobile.set(result.matches);
      this.sidenavOpened.set(!result.matches);
    });
  }

  toggleSidenav(): void {
    this.sidenavOpened.set(!this.sidenavOpened());
  }
}
