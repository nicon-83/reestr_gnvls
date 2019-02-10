
function selectPageSize() {
	let path = $('#link_for_page-size').prop('href');
	let rowsCount = $("#page-size :selected").val();
	let currentPageObj = $("#page-number");
	let userString = $("#search").val().trim();
	//let currentPage = currentPageObj.attr("value");
	let currentPage = 1;
	console.log(rowsCount);
	startLoadingAnimation();
	$.ajax({
		cache: false,
		url: path,
		type: "POST",
		data: {
			direction: $(this).attr("id"),
			currentPage: currentPage,
			rowsCount: rowsCount,
			userString: userString
		},
		success: function (data) {
			stopLoadingAnimation();
			$(".main").empty().append(data);
			currentPageObj.attr("value", 1);
			//currentPage = parseInt(currentPageObj.attr("value"));
			let totalPages = parseInt($("#total-pages-hidden").text());
			$("#total-pages").attr("value", totalPages);
			if (currentPage > totalPages) {
				currentPageObj.val(totalPages);
				currentPageObj.attr("value", totalPages);
			} else {
				currentPageObj.val(currentPage);
				currentPageObj.attr("value", currentPage);
			}
			if (currentPageObj.attr("value") == $("#total-pages").attr("value")) {
				$("#next").parent().addClass("disabled");
				$("#last").parent().addClass("disabled");
			} else {
				$("#next").parent().removeClass("disabled");
				$("#last").parent().removeClass("disabled");
			}
			if (parseInt(currentPageObj.attr("value")) > 1) {
				$("#prev").parent().removeClass("disabled");
				$("#first").parent().removeClass("disabled");
			} else {
				$("#prev").parent().addClass("disabled");
				$("#first").parent().addClass("disabled");
			}
		}
	});
}

function startLoadingAnimation() {
	let imgObj = $("#loadImg");
	imgObj.show();

	let centerY = $(window).scrollTop() + (window.innerHeight - imgObj.height()) / 2;
	let centerX = $(window).scrollLeft() + (window.innerWidth - imgObj.width()) / 2;

	imgObj.offset({ top: centerY, left: centerX });
}

function stopLoadingAnimation() {
	$("#loadImg").hide();
}

$(document).ready(function () {
	if ($("#page-number").attr("value") == 1) {
		$("#prev").parent().addClass("disabled");
		$("#first").parent().addClass("disabled");
	}
	if ($("#page-number").attr("value") == $("#total-pages").attr("value")) {
		$("#next").parent().addClass("disabled");
		$("#last").parent().addClass("disabled");
	}
}
);

$("#page-size").change(selectPageSize);

$("#next").click(function (e) {
	e.preventDefault();
	let currentPageObj = $("#page-number");
	let currentPage = currentPageObj.attr("value");
	let userString = $("#search").val().trim();
	startLoadingAnimation();
	$.ajax({
		cache: false,
		url: this.href,
		type: "POST",
		data: {
			direction: this.getAttribute("id"),
			currentPage: currentPage,
			rowsCount: $("#page-size :selected").val(),
			userString: userString
		},
		success: function (data) {
			stopLoadingAnimation();
			$(".main").empty().append(data);
			currentPage = parseInt(currentPage) + 1;
			currentPageObj.val(currentPage);
			currentPageObj.attr("value", currentPage);
			if (currentPageObj.attr("value") == $("#total-pages").attr("value")) {
				$("#next").parent().addClass("disabled");
				$("#last").parent().addClass("disabled");
			}
			if (currentPageObj.attr("value") > 1) {
				$("#prev").parent().removeClass("disabled");
				$("#first").parent().removeClass("disabled");
			}
		}
	});
});

$("#prev").click(function (e) {
	e.preventDefault();
	let currentPageObj = $("#page-number");
	let currentPage = currentPageObj.attr("value");
	let userString = $("#search").val().trim();
	startLoadingAnimation();
	$.ajax({
		cache: false,
		url: this.href,
		type: "POST",
		data: {
			direction: this.getAttribute("id"),
			currentPage: currentPage,
			rowsCount: $("#page-size :selected").val(),
			userString: userString
		},
		success: function (data) {
			stopLoadingAnimation();
			$(".main").empty().append(data);
			if (currentPage == $("#total-pages").attr("value")) {
				$("#next").parent().removeClass("disabled");
				$("#last").parent().removeClass("disabled");
			}
			currentPage = parseInt(currentPage) - 1;
			currentPageObj.val(currentPage)
			currentPageObj.attr("value", currentPage);
			if (currentPage == 1) {
				$("#prev").parent().addClass("disabled");
				$("#first").parent().addClass("disabled");
			}
		}
	});
});

$("#first").click(function (e) {
	e.preventDefault();
	let userString = $("#search").val().trim();
	startLoadingAnimation();
	$.ajax({
		cache: false,
		url: this.href,
		type: "POST",
		data: {
			direction: this.getAttribute("id"),
			rowsCount: $("#page-size :selected").val(),
			userString: userString
		},
		success: function (data) {
			stopLoadingAnimation();
			$(".main").empty().append(data);
			$("#page-number").val("1");
			$("#page-number").attr("value", "1");
			$("#prev").parent().addClass("disabled");
			$("#first").parent().addClass("disabled");
			if ($("#last").parent().hasClass("disabled")) {
				$("#last").parent().removeClass("disabled");
			}
			if ($("#next").parent().hasClass("disabled")) {
				$("#next").parent().removeClass("disabled");
			}
		}
	});
});

$("#last").click(function (e) {
	e.preventDefault();
	let userString = $("#search").val().trim();
	startLoadingAnimation();
	$.ajax({
		cache: false,
		url: this.href,
		type: "POST",
		data: {
			direction: this.getAttribute("id"),
			rowsCount: $("#page-size :selected").val(),
			userString: userString
		},
		success: function (data) {
			stopLoadingAnimation();
			$(".main").empty().append(data);
			$("#page-number").val($("#total-pages").attr("value"));
			$("#page-number").attr("value", $("#total-pages").attr("value"));
			$("#next").parent().addClass("disabled");
			$("#last").parent().addClass("disabled");
			if ($("#prev").parent().hasClass("disabled")) {
				$("#prev").parent().removeClass("disabled");
			}
			if ($("#first").parent().hasClass("disabled")) {
				$("#first").parent().removeClass("disabled");
			}
		}
	});
});

$("#select-page").click(function (e) {
	e.preventDefault();
	let currentPageObj = $("#page-number");
	let currentPage = currentPageObj.val();
	currentPage = currentPage.replace(/\s+/g, ''); //убираем пробелы с начала и конца строки
	let userString = $("#search").val().trim();
	if (isNaN(currentPage) || ~currentPage.indexOf(".") || ~currentPage.indexOf(",") || ~currentPage.indexOf("+") || currentPage == "" || parseInt(currentPage) > parseInt($("#total-pages").attr("value")) || parseInt(currentPage) < 1) {
		alert("Внимание! Введен недопустимый номер страницы!");
		return;
	}
	startLoadingAnimation();
	$.ajax({
		cache: false,
		url: this.href,
		type: "POST",
		data: {
			direction: this.getAttribute("id"),
			currentPage: currentPage,
			rowsCount: $("#page-size :selected").val(),
			userString: userString
		},
		success: function (data) {
			stopLoadingAnimation();
			$(".main").empty().append(data);
			currentPageObj.val(currentPage)
			currentPageObj.attr("value", currentPage);
			$("#search").val(userString);
			if ($("#page-number").val() == 1) {
				$("#prev").parent().addClass("disabled");
				$("#first").parent().addClass("disabled");
				$("#next").parent().removeClass("disabled");
				$("#last").parent().removeClass("disabled");
			}
			else if ($("#page-number").val() == $("#total-pages").attr("value")) {
				$("#next").parent().addClass("disabled");
				$("#last").parent().addClass("disabled");
				$("#prev").parent().removeClass("disabled");
				$("#first").parent().removeClass("disabled");
			}
			else if ($("#page-number").val() == 1 && $("#total-pages").attr("value") == 1) {
				$("#next").parent().addClass("disabled");
				$("#last").parent().addClass("disabled");
				$("#prev").parent().addClass("disabled");
				$("#first").parent().addClass("disabled");
			}
			else {
				$("#next").parent().removeClass("disabled");
				$("#last").parent().removeClass("disabled");
				$("#prev").parent().removeClass("disabled");
				$("#first").parent().removeClass("disabled");
			}
		}
	});
});

$("#button_for_search").click(function (e) {
	let string = $("#search").val().trim();
	startLoadingAnimation();
	$.ajax({
		cache: false,
		url: $("#link_for_search_button").prop("href"),
		type: "POST",
		data: {
			userString: string,
		},
		success: function (data) {
			stopLoadingAnimation();
			$(".main").empty().append(data);
			let currentPageObj = $("#page-number");
			let currentPage = 1;
			currentPageObj.attr("value", 1);
			let totalPages = parseInt($("#total-pages-hidden").text());
			$("#total-pages").attr("value", totalPages);
			$("#page-size").val("20");

			if (currentPage > totalPages) {
				currentPageObj.val(totalPages);
				currentPageObj.attr("value", totalPages);
			} else {
				currentPageObj.val(currentPage);
				currentPageObj.attr("value", currentPage);
			}
			if (currentPageObj.attr("value") == $("#total-pages").attr("value")) {
				$("#next").parent().addClass("disabled");
				$("#last").parent().addClass("disabled");
			} else {
				$("#next").parent().removeClass("disabled");
				$("#last").parent().removeClass("disabled");
			}
			if (parseInt(currentPageObj.attr("value")) > 1) {
				$("#prev").parent().removeClass("disabled");
				$("#first").parent().removeClass("disabled");
			} else {
				$("#prev").parent().addClass("disabled");
				$("#first").parent().addClass("disabled");
			}
		}
	});
});

$("#download-csv").click(function () {
	window.location.href = $("#link_for_download_csv").prop("href");
	//startLoadingAnimation();
	//$.ajax({
	//	cache: false,
	//	url: $("#link_for_download_csv").prop("href"),
	//	type: "GET",
	//	complete: function () {
	//		stopLoadingAnimation();
	//	}
	//});
});