import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { CreateCVComponent } from './pages/create-cv/create-cv.component';
import { AuthPageComponent } from './pages/auth-page/auth-page.component';
import { AuthGuard } from './guards/auth.guard';
import { MyCVsComponent } from './pages/my-cvs/my-cvs.component';
import { CVDetailsComponent } from './pages/cv-details/cv-details.component';
import { CVEditComponent } from './pages/cv-edit/cv-edit.component';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';

const routes: Routes = [
  {
    path: 'auth',
    component: AuthPageComponent,
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      { path: 'home', component: HomeComponent },
      { path: 'cv/create', component: CreateCVComponent },
      { path: 'my-cvs', component: MyCVsComponent },
      { path: 'cv/:id', component: CVDetailsComponent },
      { path: 'cv/edit/:id', component: CVEditComponent },
      { path: '', redirectTo: 'home', pathMatch: 'full' },
    ],
  },
  {
    path: '**',
    redirectTo: 'home',
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
