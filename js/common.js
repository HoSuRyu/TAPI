$(document).ready(function(){
	"use strict";
	
	/*Sidebar toggle*/
	var sidebarOpen = function(){
		if(!$('.bkcover').is('.bkcover_on')){
				$('.bkcover').addClass('bkcover_on');
				$('.bkcover').animate({opacity:0.8}, 200, 'easeOutQuart');
			} else {
				$('.bkcover').animate({opacity:0}, 200, 'easeOutQuart', function(){
					$('.bkcover').removeClass('bkcover_on');
				});
			}
		$('.sidebar').toggleClass('sidebar_view');
		$('#main-content').toggleClass('bar_area_view');
	};
	
	
	/*클릭시 Sidebar toggle 작동*/
	$('.sidebar-toggle-box .fa-bars, .bkcoverin').click(function (e) {
		sidebarOpen();
		e.stopPropagation();
	});
	
	
	/*PC 사이즈일 경우 Sidebar toggle 작동*/
	var winwidth = $(window).width();
	if(winwidth>=992){
		sidebarOpen();
	}
	
	/*sidebar애니메이션용 css 추가*/
	setTimeout($.proxy(function(){
		$('.sidebar').addClass('sidebar_trans');
		$('#main-content').addClass('bar_area_trans');
	},this),100);




	
});