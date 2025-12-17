import { Component, Input, output } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';

@Component({
  selector: 'app-category-nav',
  standalone: true,
  imports: [CommonModule, NgClass],
  templateUrl: './category-nav.component.html',
  styleUrl: './category-nav.component.scss'
})
export class CategoryNavComponent {
  @Input({ required: true }) categories: string[] = [];
  @Input({ required: true }) selectedCategory: string = '';
  
  categorySelected = output<string>();

  isFoodCategory(category: string): boolean {
    const foodKeywords = ['food', 'burger', 'pizza', 'pasta', 'sushi', 'appetizer', 'main', 'combo'];
    const categoryLower = category.toLowerCase();
    return foodKeywords.some(keyword => categoryLower.includes(keyword)) || categoryLower === 'all';
  }

  isDrinkCategory(category: string): boolean {
    const drinkKeywords = ['drink', 'drinks', 'beverage', 'cola', 'water', 'juice'];
    const categoryLower = category.toLowerCase();
    return drinkKeywords.some(keyword => categoryLower.includes(keyword));
  }

  onCategoryClick(category: string): void {
    this.categorySelected.emit(category);
  }
}
