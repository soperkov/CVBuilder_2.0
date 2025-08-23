import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { CreateCVComponent } from './pages/create-cv/create-cv.component';
import { AuthPageComponent } from './pages/auth-page/auth-page.component';
import { AuthGuard } from './guards/auth.guard';
import { MyCVsComponent } from './pages/my-cvs/my-cvs.component';
import { CVDetailsComponent } from './pages/cv-details/cv-details.component';
import { CVEditComponent } from './pages/cv-edit/cv-edit.component';
import { RedirectComponent } from './pages/redirect/redirect.component';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';

const routes: Routes = [
  {
    path: '',
    component: RedirectComponent,
  },
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
      { path: 'cv/:id', component: CVDetailsComponent },
      { path: 'cv/edit/:id', component: CVEditComponent },
      { path: 'my-cvs', component: MyCVsComponent },
    ],
  },
  {
    path: 'home',
    component: HomeComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'cv/create',
    component: CreateCVComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'my-cvs',
    component: MyCVsComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'cv/:id',
    component: CVDetailsComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'cv/edit/:id',
    component: CVEditComponent,
    canActivate: [AuthGuard],
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
