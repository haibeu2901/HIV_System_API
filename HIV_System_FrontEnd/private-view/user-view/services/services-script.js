document.addEventListener('DOMContentLoaded', function() {
    // Initialize services page
    loadMedicalServices();
    setupFilterButtons();
    setupBookingButtons();
});

// Global variables
let allServices = [];
let filteredServices = [];
let currentFilter = 'all';

// Load medical services from API
async function loadMedicalServices() {
    const servicesGrid = document.getElementById('servicesGrid');
    
    try {
        console.log('Loading medical services...');
        
        // Show loading state
        servicesGrid.innerHTML = `
            <div class="services-loading">
                <i class="fas fa-spinner fa-spin"></i>
                <p>Đang tải dịch vụ...</p>
            </div>
        `;

        const response = await fetch('https://localhost:7009/api/MedicalService/GetAllMedicalServices', {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const services = await response.json();
        console.log('Medical services loaded:', services);
        
        allServices = services.filter(service => service.isAvailable);
        filteredServices = [...allServices];
        
        renderServices();

    } catch (error) {
        console.error('Error loading medical services:', error);
        showErrorState();
    }
}

// Render services based on current filter
function renderServices() {
    const servicesGrid = document.getElementById('servicesGrid');
    
    if (filteredServices.length === 0) {
        servicesGrid.innerHTML = `
            <div class="services-empty">
                <i class="fas fa-search"></i>
                <p>Không tìm thấy dịch vụ nào phù hợp với bộ lọc.</p>
            </div>
        `;
        return;
    }

    servicesGrid.innerHTML = filteredServices.map(service => {
        const icon = getServiceIcon(service.serviceName);
        const category = getServiceCategory(service.serviceName);
        
        return `
            <div class="service-card" data-service-id="${service.serviceId}" data-category="${category}">
                <div class="service-icon">
                    <i class="${icon}"></i>
                </div>
                <h3>${service.serviceName}</h3>
                <p class="service-description">${service.serviceDescription}</p>
                <div class="service-price">${formatPrice(service.price)}</div>
                <div class="service-availability ${service.isAvailable ? '' : 'unavailable'}">
                    <i class="fas fa-${service.isAvailable ? 'check-circle' : 'times-circle'}"></i>
                    ${service.isAvailable ? 'Có sẵn' : 'Tạm ngưng'}
                </div>
                <div class="service-actions">
                    <button class="btn-view-detail" onclick="viewServiceDetail(${service.serviceId})">
                        <i class="fas fa-info-circle"></i> Chi tiết
                    </button>
                    <button class="btn-book-service" onclick="bookService(${service.serviceId})" ${!service.isAvailable ? 'disabled' : ''}>
                        <i class="fas fa-calendar-plus"></i> Đặt lịch
                    </button>
                </div>
            </div>
        `;
    }).join('');
}

// Show error state
function showErrorState() {
    const servicesGrid = document.getElementById('servicesGrid');
    servicesGrid.innerHTML = `
        <div class="services-error">
            <i class="fas fa-exclamation-triangle"></i>
            <p>Không thể tải dịch vụ. Vui lòng thử lại sau.</p>
            <button class="retry-btn" onclick="loadMedicalServices()">
                <i class="fas fa-redo"></i> Thử lại
            </button>
        </div>
    `;
}

// Setup filter buttons
function setupFilterButtons() {
    const filterButtons = document.querySelectorAll('.filter-btn');
    
    filterButtons.forEach(button => {
        button.addEventListener('click', function() {
            // Remove active class from all buttons
            filterButtons.forEach(btn => btn.classList.remove('active'));
            
            // Add active class to clicked button
            this.classList.add('active');
            
            // Apply filter
            const filter = this.dataset.filter;
            applyFilter(filter);
        });
    });
}

// Apply filter to services
function applyFilter(filter) {
    currentFilter = filter;
    
    if (filter === 'all') {
        filteredServices = [...allServices];
    } else {
        filteredServices = allServices.filter(service => {
            const category = getServiceCategory(service.serviceName);
            return category === filter;
        });
    }
    
    renderServices();
}

// Get service category based on service name
function getServiceCategory(serviceName) {
    const testingKeywords = ['xét nghiệm', 'test', 'tải lượng', 'cd4'];
    const consultationKeywords = ['tư vấn', 'consultation'];
    const treatmentKeywords = ['điều trị', 'arv', 'treatment'];
    
    const name = serviceName.toLowerCase();
    
    if (testingKeywords.some(keyword => name.includes(keyword))) {
        return 'testing';
    } else if (consultationKeywords.some(keyword => name.includes(keyword))) {
        return 'consultation';
    } else if (treatmentKeywords.some(keyword => name.includes(keyword))) {
        return 'treatment';
    }
    
    return 'other';
}

// Get appropriate icon for service
function getServiceIcon(serviceName) {
    const iconMap = {
        'Xét nghiệm HIV Combo': 'fas fa-vial',
        'Tư vấn PrEP': 'fas fa-shield-alt',
        'Điều trị ARV': 'fas fa-pills',
        'Xét nghiệm tải lượng HIV': 'fas fa-microscope',
        'Đếm CD4': 'fas fa-dna',
        'Tư vấn HIV': 'fas fa-comments',
        'Tư vấn PEP': 'fas fa-first-aid'
    };
    
    return iconMap[serviceName] || 'fas fa-stethoscope';
}

// Format price to Vietnamese currency
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
}

// View service detail in modal
function viewServiceDetail(serviceId) {
    const service = allServices.find(s => s.serviceId === serviceId);
    if (!service) return;
    
    // Populate modal with service details
    document.getElementById('modalServiceName').textContent = service.serviceName;
    document.getElementById('modalServiceDescription').textContent = service.serviceDescription;
    document.getElementById('modalServicePrice').textContent = formatPrice(service.price);
    
    const modalIcon = document.getElementById('modalServiceIcon');
    modalIcon.innerHTML = `<i class="${getServiceIcon(service.serviceName)}"></i>`;
    
    const availabilityElement = document.getElementById('modalServiceAvailability');
    if (service.isAvailable) {
        availabilityElement.innerHTML = '<i class="fas fa-check-circle"></i> Có sẵn';
        availabilityElement.className = 'service-availability';
    } else {
        availabilityElement.innerHTML = '<i class="fas fa-times-circle"></i> Tạm ngưng';
        availabilityElement.className = 'service-availability unavailable';
    }
    
    // Setup book button in modal
    const bookBtn = document.getElementById('bookServiceBtn');
    bookBtn.onclick = () => {
        closeServiceModal();
        bookService(serviceId);
    };
    bookBtn.disabled = !service.isAvailable;
    
    // Show modal
    document.getElementById('serviceModal').style.display = 'block';
    document.body.style.overflow = 'hidden';
}

// Close service detail modal
function closeServiceModal() {
    document.getElementById('serviceModal').style.display = 'none';
    document.body.style.overflow = 'auto';
}

// Book service - redirect to booking page
function bookService(serviceId) {
    const service = allServices.find(s => s.serviceId === serviceId);
    if (!service || !service.isAvailable) {
        alert('Dịch vụ này hiện tại không khả dụng.');
        return;
    }
    
    // Store selected service info for booking page
    localStorage.setItem('selectedService', JSON.stringify({
        serviceId: service.serviceId,
        serviceName: service.serviceName,
        serviceDescription: service.serviceDescription,
        price: service.price
    }));
    
    // Redirect to booking page
    window.location.href = '../booking/appointment-booking.html';
}

// Setup booking buttons
function setupBookingButtons() {
    // Close modal when clicking outside
    window.addEventListener('click', function(event) {
        const modal = document.getElementById('serviceModal');
        if (event.target === modal) {
            closeServiceModal();
        }
    });
    
    // Close modal with Escape key
    document.addEventListener('keydown', function(event) {
        if (event.key === 'Escape') {
            closeServiceModal();
        }
    });
}

// Search functionality (can be added later)
function searchServices(searchTerm) {
    if (!searchTerm.trim()) {
        filteredServices = [...allServices];
    } else {
        filteredServices = allServices.filter(service => 
            service.serviceName.toLowerCase().includes(searchTerm.toLowerCase()) ||
            service.serviceDescription.toLowerCase().includes(searchTerm.toLowerCase())
        );
    }
    
    renderServices();
}

// Export functions for global use
window.viewServiceDetail = viewServiceDetail;
window.closeServiceModal = closeServiceModal;
window.bookService = bookService;
window.loadMedicalServices = loadMedicalServices;
