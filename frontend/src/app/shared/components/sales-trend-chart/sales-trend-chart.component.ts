import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { SalesTrend } from '../../../core/models/dashboard.model';

@Component({
  selector: 'app-sales-trend-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './sales-trend-chart.component.html',
  styleUrl: './sales-trend-chart.component.scss'
})
export class SalesTrendChartComponent implements OnInit, OnChanges {
  @Input() data: SalesTrend[] = [];
  @Input() title: string = 'Sales Trend';

  public chartType: ChartType = 'line';
  public chartData: ChartData<'line'> = {
    labels: [],
    datasets: []
  };
  public chartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'top'
      },
      title: {
        display: true,
        text: this.title
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: function(value) {
            return 'RM ' + value;
          }
        }
      }
    }
  };

  ngOnInit(): void {
    this.updateChart();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data']) {
      this.updateChart();
    }
    if (changes['title']) {
      this.chartOptions = {
        ...this.chartOptions,
        plugins: {
          ...this.chartOptions?.plugins,
          title: {
            display: true,
            text: this.title
          }
        }
      };
    }
  }

  private updateChart(): void {
    if (!this.data || this.data.length === 0) {
      this.chartData = {
        labels: [],
        datasets: []
      };
      return;
    }

    const labels = this.data.map(d => {
      const date = new Date(d.date);
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    });

    this.chartData = {
      labels,
      datasets: [
        {
          label: 'Revenue',
          data: this.data.map(d => d.revenue),
          borderColor: 'rgb(75, 192, 192)',
          backgroundColor: 'rgba(75, 192, 192, 0.2)',
          tension: 0.3,
          fill: true
        }
      ]
    };
  }
}

