// 🚗 Smart Parking Management System (SPMS) Custom JS

document.addEventListener('DOMContentLoaded', function() {
    // 1. Hiệu ứng đổi màu header khi cuộn trang
    const navbar = document.querySelector('.navbar-custom');
    
    function checkScroll() {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    }
    
    window.addEventListener('scroll', checkScroll);
    checkScroll(); // Chạy kiểm tra ngay khi load xong trang

    // 2. Smooth scroll cho các liên kết anchor nội bộ
    const links = document.querySelectorAll('a[href^="#"]');
    for (const link of links) {
        link.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            if (href !== "#") {
                e.preventDefault();
                const target = document.querySelector(href);
                if (target) {
                    const offsetTop = target.offsetTop - 80; // Trừ đi chiều cao header
                    window.scrollTo({
                        top: offsetTop,
                        behavior: 'smooth'
                    });
                }
            }
        });
    }
});
