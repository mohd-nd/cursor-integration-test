import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WeatherService, WeatherData } from './weather.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private weatherService = inject(WeatherService);
  
  weather = signal<WeatherData | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  lastUpdated = signal<Date | null>(null);

  ngOnInit(): void {
    this.fetchWeather();
  }

  fetchWeather(): void {
    this.loading.set(true);
    this.error.set(null);
    
    this.weatherService.getAmmanWeather().subscribe({
      next: (data) => {
        this.weather.set(data);
        this.lastUpdated.set(new Date());
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to fetch weather data. Please try again.');
        this.loading.set(false);
        console.error('Weather fetch error:', err);
      }
    });
  }

  getWeatherDescription(code: number): string {
    return this.weatherService.getWeatherDescription(code);
  }

  getWeatherIcon(code: number): string {
    return this.weatherService.getWeatherIcon(code);
  }
}
