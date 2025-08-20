import { Router } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit {
  userInfo: any = null;
  errorMessage: string | null = null;

  constructor(private userService: UserService, private router: Router) {}

  ngOnInit(): void {
    this.userService.getUserData().subscribe({
      next: (data) => {
        this.userInfo = data;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load user info.';
      },
    });
  }

  createCV() {
    this.router.navigate(['/cv/create']);
  }

  goToMyCVs() {
    this.router.navigate(['/my-cvs']);
  }
}
