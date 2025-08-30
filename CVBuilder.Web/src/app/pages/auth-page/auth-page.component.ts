import { Component, OnInit } from '@angular/core';

type Gear = {
  size: number; // px
  top: number; // %
  left: number; // %
  duration: number; // s
  clockwise: boolean;
  opacity: number;
};

@Component({
  selector: 'app-auth-page',
  standalone: false,
  templateUrl: './auth-page.component.html',
  styleUrl: './auth-page.component.css',
})
export class AuthPageComponent {
  showLogin = true;

  gears: Gear[] = [];

  toggleForm(): void {
    this.showLogin = !this.showLogin;
  }

  ngOnInit(): void {
    // base gears (a few fixed anchors so it doesn’t feel random on first render)
    const anchors: Gear[] = [
      {
        size: 110,
        top: 10,
        left: 14,
        duration: 22,
        clockwise: true,
        opacity: 0.65,
      },
      {
        size: 150,
        top: 18,
        left: 78,
        duration: 18,
        clockwise: false,
        opacity: 0.6,
      },
      {
        size: 80,
        top: 72,
        left: 12,
        duration: 28,
        clockwise: true,
        opacity: 0.55,
      },
      {
        size: 120,
        top: 78,
        left: 71,
        duration: 20,
        clockwise: false,
        opacity: 0.6,
      },
      {
        size: 70,
        top: 40,
        left: 30,
        duration: 24,
        clockwise: true,
        opacity: 0.6,
      },
      {
        size: 95,
        top: 44,
        left: 62,
        duration: 21,
        clockwise: false,
        opacity: 0.6,
      },
    ];
    this.gears.push(...anchors);

    // add lots of soft background gears
    const extra = 26; // tweak to taste
    for (let i = 0; i < extra; i++) {
      this.gears.push({
        size: Math.floor(Math.random() * 70) + 40, // 40–110
        top: Math.floor(Math.random() * 96) + 2, // 2–98
        left: Math.floor(Math.random() * 96) + 2, // 2–98
        duration: Math.floor(Math.random() * 18) + 16, // 16–34s
        clockwise: Math.random() > 0.5,
        opacity: Math.random() * 0.35 + 0.45, // 0.45–0.8
      });
    }
  }
}
