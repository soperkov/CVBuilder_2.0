import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthInterceptor } from './interceptors/auth.interceptor';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HomeComponent } from './pages/home/home.component';
import { CreateCVComponent } from './pages/create-cv/create-cv.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { UserInfoComponent } from './shared/user-info/user-info.component';
import { AuthPageComponent } from './pages/auth-page/auth-page.component';
import { CvFormComponent } from './components/cv-form/cv-form.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    CreateCVComponent,
    LoginComponent,
    RegisterComponent,
    UserInfoComponent,
    AuthPageComponent,
    CvFormComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
