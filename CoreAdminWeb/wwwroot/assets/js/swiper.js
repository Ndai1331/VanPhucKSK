function Swiperload() {
    var swiperDefault = new Swiper(".SwiperDefault", {});

    var swiperWithArrows = new Swiper(".SwiperwithArrows", {
        loop: true,
        navigation: {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
    });

    var swiperWithPagination = new Swiper(".SwiperwithPagination", {
        pagination: {
            el: ".swiper-pagination",
        },
    });

    var swiperWithProgress = new Swiper(".SwiperwithProgress", {
        pagination: {
            el: ".swiper-pagination",
            type: "progressbar",
        },
        navigation: {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
    });

    var swiperMultiple = new Swiper(".SwiperMultiple", {
        slidesPerView: 1,
        spaceBetween: 30,
        pagination: {
            el: ".swiper-pagination",
            clickable: true,
        },
        breakpoints: {
            768: {
                slidesPerView: 2,
                spaceBetween: 30
            }
        }
    });
}
