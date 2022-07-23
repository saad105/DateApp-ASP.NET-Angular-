import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
  title = 'Date App';
  Users : any;
  constructor(private http: HttpClient) {
  }
  
  ngOnInit(): void {
    this.getusers();
  }

  getusers(){
    this.http.get('https://localhost:5001/api/users').subscribe(response => {
      this.Users = response;
    },error => {
      console.log(error);
    })
  }
}
