import { Time } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FlightService,  } from './../api/services/flight.service';
import { FlightRm } from '../api/models';
import { FormBuilder } from '@angular/forms';
import { SearchFlight$Params } from './../api/fn/flight/search-flight'

@Component({
  selector: 'app-search-flights',
  templateUrl: './search-flights.component.html',
  styleUrls: ['./search-flights.component.css']
})
export class SearchFlightsComponent implements OnInit {


  searchResult: FlightRm[] = []

  constructor(private flightService: FlightService,
    private fb: FormBuilder) { }

  searchForm = this.fb.group({
    from: [''],
    destination: [''],
    fromDate: [''],
    toDate: [''],
    numberOfPassengers: [1]
  })

  ngOnInit(): void {
    this.search();
  }

  search() {
    const searchParams: SearchFlight$Params = {
      from: this.searchForm.get('from')?.value,
      destination: this.searchForm.get('destination')?.value,
      fromDate: this.searchForm.get('fromDate')?.value,
      toDate: this.searchForm.get('toDate')?.value,
      numberOfPassengers: this.searchForm.get('numberOfPassengers')?.value,
    };

    this.flightService.searchFlight(searchParams)
      .subscribe(response => this.searchResult = response, this.handleError);
  }

  private handleError(err: any) {
    console.log("Response Error. Status: ", err.status)
    console.log("Response Error. Status Text: ", err.statusText)
    console.log(err)
  }

}
