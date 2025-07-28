import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { CreateCVComponent } from './pages/create-cv/create-cv.component';

const routes: Routes = [
  { path: '', component: HomeComponent },
 { path: 'cv/create', component: CreateCVComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
