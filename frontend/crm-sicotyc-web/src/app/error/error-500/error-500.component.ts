import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-error-500',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './error-500.component.html',
  styles: [`
    :host {
      display: flex;
      min-height: 100vh;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #f8fafc 0%, #eef2ff 100%);
      padding: 24px;
    }

    .card {
      width: min(100%, 560px);
      border-radius: 20px;
      background: white;
      box-shadow: 0 20px 45px rgba(15, 23, 42, 0.12);
      padding: 32px;
      text-align: center;
    }

    .code {
      font-size: 4rem;
      font-weight: 800;
      color: #ea580c;
      margin-bottom: 8px;
    }

    .actions {
      display: flex;
      justify-content: center;
      gap: 12px;
      flex-wrap: wrap;
      margin-top: 20px;
    }

    a {
      text-decoration: none;
      padding: 10px 16px;
      border-radius: 999px;
      font-weight: 600;
      transition: transform 0.2s ease;
    }

    a:hover {
      transform: translateY(-1px);
    }

    .primary {
      background: #4f46e5;
      color: white;
    }

    .secondary {
      background: #e2e8f0;
      color: #0f172a;
    }
  `]
})
export class Error500Component {}
