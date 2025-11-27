import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';

@Component({
  selector: 'app-bar-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './bar-chart.component.html',
  styleUrl: './bar-chart.component.scss'
})
export class BarChartComponent implements OnInit, OnChanges {
  @Input() data: { label: string; value: number }[] = [];
  @Input() title: string = 'Bar Chart';
  @Input() orientation: 'horizontal' | 'vertical' = 'vertical';
  @Input() yAxisLabel?: string;

  public chartType: ChartType = 'bar';
  public chartData: ChartData<'bar'> = {
    labels: [],
    datasets: []
  };
  public chartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: this.orientation === 'horizontal' ? 'y' : 'x',
    plugins: {
      legend: {
        display: false
      },
      title: {
        display: true,
        text: this.title
      }
    },
    scales: {
      y: {
        beginAtZero: true
      }
    }
  };

  ngOnInit(): void {
    this.updateChart();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] || changes['orientation']) {
      this.updateChart();
    }
    if (changes['title'] || changes['yAxisLabel']) {
      this.chartOptions = {
        ...this.chartOptions,
        plugins: {
          ...this.chartOptions?.plugins,
          title: {
            display: true,
            text: this.title
          }
        },
        scales: {
          ...this.chartOptions?.scales,
          y: {
            beginAtZero: true,
            title: {
              display: !!this.yAxisLabel,
              text: this.yAxisLabel
            }
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

    this.chartOptions = {
      ...this.chartOptions,
      indexAxis: this.orientation === 'horizontal' ? 'y' : 'x'
    };

    this.chartData = {
      labels: this.data.map(d => d.label),
      datasets: [
        {
          label: this.yAxisLabel || 'Value',
          data: this.data.map(d => d.value),
          backgroundColor: 'rgba(54, 162, 235, 0.8)',
          borderColor: 'rgba(54, 162, 235, 1)',
          borderWidth: 1
        }
      ]
    };
  }
}

