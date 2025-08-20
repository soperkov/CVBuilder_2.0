import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { AuthInterceptor } from './interceptors/auth.interceptor';

// Pages
import { HomeComponent } from './pages/home/home.component';
import { CreateCVComponent } from './pages/create-cv/create-cv.component';
import { AuthPageComponent } from './pages/auth-page/auth-page.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { MyCVsComponent } from './pages/my-cvs/my-cvs.component';
import { CVDetailsComponent } from './pages/cv-details/cv-details.component';

// Components
import { NavbarComponent } from './components/navbar/navbar.component';
import { CVFormComponent } from './components/cv-form/cv-form.component';
import { UserInfoComponent } from './shared/user-info/user-info.component';
import { CVEditComponent } from './pages/cv-edit/cv-edit.component';
import { RedirectComponent } from './pages/redirect/redirect.component';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { CvNameModalComponent } from './shared/cv-name-modal/cv-name-modal.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    CreateCVComponent,
    AuthPageComponent,
    LoginComponent,
    RegisterComponent,
    NavbarComponent,
    CVFormComponent,
    UserInfoComponent,
    MyCVsComponent,
    CVDetailsComponent,
    CVEditComponent,
    RedirectComponent,
    MainLayoutComponent,
    CvNameModalComponent,
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
