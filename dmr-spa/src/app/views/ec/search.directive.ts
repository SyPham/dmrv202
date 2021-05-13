import { Directive, AfterViewInit, ElementRef, Input, OnChanges, HostListener, OnInit, OnDestroy } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

@Directive({
  // tslint:disable-next-line:directive-selector
  selector: '[autofocusSearch]'
})
export class SearchDirective implements AfterViewInit, OnInit, OnDestroy {
  subject = new Subject<string>();
  subscription: Subscription[] = [];
  @HostListener('focus') onFocus() {
    setTimeout(() => {
      this.host.nativeElement.select();
    }, 300);
  }
  @HostListener('focusout') onFocusout() {
    setTimeout(() => {
      this.host.nativeElement.focus();
    }, 2000);
  }
  @HostListener('ngModelChange', ['$event']) onChange(value) {
    this.subject.next(value);
  }
  constructor(private host: ElementRef) { }
  ngAfterViewInit() {
    setTimeout(() => {
      this.host.nativeElement.focus();
    }, 300);
  }
  ngOnInit() {
    this.subscription.push(this.subject
      .pipe(debounceTime(300))
      .subscribe(async (arg) => {
        this.host.nativeElement.select();
      }));
  }
  ngOnDestroy() {
    this.subscription.forEach(item => item.unsubscribe());
  }
  @HostListener('document:keydown.enter', ['$event'])
  onKeydownHandler(event: KeyboardEvent) {
    event.preventDefault();
    this.host.nativeElement.value = this.host.nativeElement.value + '    ';
  }
  @HostListener('document:keydown.tab', ['$event'])
  onKeydownTabHandler(event: KeyboardEvent) {
    event.preventDefault();
    this.host.nativeElement.value = this.host.nativeElement.value + '    ';
  }
}
