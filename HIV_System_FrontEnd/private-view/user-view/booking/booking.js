document.addEventListener('DOMContentLoaded', function() {
    // Check if user came from services page with selected service
    checkSelectedService();
    
    // Tab buttons
    const byTimeBtn = document.getElementById('by-time-btn');
    const byDoctorBtn = document.getElementById('by-doctor-btn');
    const byTimeSection = document.getElementById('by-time-section');
    const byDoctorSection = document.getElementById('by-doctor-section');

    // By Time elements
    const calendarDate = document.getElementById('calendar-date');
    const timeSlotGrid = document.getElementById('time-slot-grid');
    const availableDoctorsSection = document.getElementById('available-doctors-section');
    const doctorList = document.getElementById('doctor-list');
    const confirmTimeBookingBtn = document.getElementById('confirm-time-booking-btn');

    // By Doctor elements
    const doctorSelect = document.getElementById('doctor-select');
    const doctorScheduleSection = document.getElementById('doctor-schedule-section');
    const doctorScheduleDate = document.getElementById('doctor-schedule-date');
    const doctorTimeSlotGrid = document.getElementById('doctor-time-slot-grid');
    const confirmDoctorBookingBtn = document.getElementById('confirm-doctor-booking-btn');

    // State
    let selectedTimeSlot = null;
    let selectedDoctor = null;
    let selectedDoctorId = null;
    let selectedDoctorTimeSlot = null;
    let selectedTimeBtn = null;
    let selectedService = null; // Store selected service info

    // Check for selected service from services page
    function checkSelectedService() {
        const serviceData = localStorage.getItem('selectedService');
        if (serviceData) {
            try {
                selectedService = JSON.parse(serviceData);
                console.log('Selected service from services page:', selectedService);
                
                // Show selected service info
                showSelectedServiceInfo();
                
                // Clear the stored service data
                localStorage.removeItem('selectedService');
            } catch (error) {
                console.error('Error parsing selected service data:', error);
                localStorage.removeItem('selectedService');
            }
        }
    }

    // Show selected service information
    function showSelectedServiceInfo() {
        if (!selectedService) return;
        
        // Create and show selected service banner
        const serviceInfoBanner = document.createElement('div');
        serviceInfoBanner.className = 'selected-service-banner';
        serviceInfoBanner.innerHTML = `
            <div class="selected-service-content">
                <div class="service-info">
                    <h3><i class="fas fa-stethoscope"></i> Dịch vụ đã chọn</h3>
                    <div class="service-details">
                        <div class="service-name">${selectedService.serviceName}</div>
                        <div class="service-description">${selectedService.serviceDescription}</div>
                        <div class="service-price">${formatPrice(selectedService.price)}</div>
                    </div>
                </div>
                <button class="btn-remove-service" onclick="removeSelectedService()">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
        
        // Insert banner at the top of booking container
        const bookingContainer = document.querySelector('.booking-container');
        const sectionTitle = bookingContainer.querySelector('.section-title');
        bookingContainer.insertBefore(serviceInfoBanner, sectionTitle.nextSibling);
    }

    // Remove selected service
    window.removeSelectedService = function() {
        selectedService = null;
        const banner = document.querySelector('.selected-service-banner');
        if (banner) {
            banner.remove();
        }
    };

    // Format price to Vietnamese currency
    function formatPrice(price) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(price);
    }

    // Tab switching
    byTimeBtn.addEventListener('click', function() {
        byTimeBtn.classList.add('active');
        byDoctorBtn.classList.remove('active');
        byTimeSection.style.display = 'block';
        byDoctorSection.style.display = 'none';
    });

    byDoctorBtn.addEventListener('click', function() {
        byDoctorBtn.classList.add('active');
        byTimeBtn.classList.remove('active');
        byDoctorSection.style.display = 'block';
        byTimeSection.style.display = 'none';
    });

    // --- Order by Time Section ---
    async function fetchAvailableTimeSlots(date) {
        const token = localStorage.getItem('token');
        
        if (!token) {
            console.error('No token found');
            return [];
        }
        
        try {
            console.log('Fetching available time slots for date:', date);
            
            // Validate date format
            if (!date || !date.match(/^\d{4}-\d{2}-\d{2}$/)) {
                console.error('Invalid date format:', date);
                return [];
            }
            
            // Generate all possible time slots for the day (8:00 AM to 6:00 PM)
            const dayStartTime = '08:00';
            const dayEndTime = '18:00';
            const allPossibleTimeSlots = generateTimeSlotsBetween(dayStartTime, dayEndTime);
            
            console.log('All possible time slots for the day:', allPossibleTimeSlots);
            
            if (allPossibleTimeSlots.length === 0) {
                console.warn('No time slots generated');
                return [];
            }
            
            // Get all doctors to check their schedules
            const doctorsResponse = await fetch('https://localhost:7009/api/Doctor/GetAllDoctors', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (!doctorsResponse.ok) {
                console.error('Failed to fetch doctors:', doctorsResponse.status);
                throw new Error(`HTTP error! status: ${doctorsResponse.status}`);
            }
            
            const doctors = await doctorsResponse.json();
            console.log('All doctors:', doctors);
            
            if (!doctors || doctors.length === 0) {
                console.warn('No doctors found');
                return [];
            }
            
            // Fetch all doctor schedules for the selected date (optimize by fetching once per doctor)
            const doctorSchedules = [];
            
            console.log(`Fetching schedules for ${doctors.length} doctors on date: ${date}`);
            
            for (const doctor of doctors) {
                try {
                    if (!doctor.doctorId) {
                        console.warn('Doctor missing doctorId:', doctor);
                        continue;
                    }
                    
                    console.log(`Fetching schedule for doctor ${doctor.doctorId}...`);
                    
                    const scheduleResponse = await fetch(`https://localhost:7009/api/DoctorWorkSchedule/GetDoctorWorkSchedulesByDoctorId/${doctor.doctorId}`, {
                        headers: {
                            'Authorization': `Bearer ${token}`,
                            'accept': '*/*'
                        }
                    });
                    
                    if (scheduleResponse.ok) {
                        const schedules = await scheduleResponse.json();
                        console.log(`Schedules for doctor ${doctor.doctorId}:`, schedules);
                        
                        const schedule = schedules.find(s => s.workDate === date);
                        
                        if (schedule && schedule.isAvailable) {
                            console.log(`Doctor ${doctor.doctorId} is available on ${date}:`, schedule);
                            doctorSchedules.push({
                                doctorId: doctor.doctorId,
                                startTime: schedule.startTime.substring(0, 5),
                                endTime: schedule.endTime.substring(0, 5)
                            });
                        } else {
                            console.log(`Doctor ${doctor.doctorId} not available on ${date}:`, schedule);
                        }
                    } else {
                        console.warn(`Failed to fetch schedule for doctor ${doctor.doctorId}:`, scheduleResponse.status);
                    }
                } catch (error) {
                    console.warn(`Error fetching schedule for doctor ${doctor.doctorId}:`, error);
                }
            }
            
            console.log('All available doctor schedules:', doctorSchedules);
            
            // Check which time slots have at least one doctor available
            const availableTimeSlots = allPossibleTimeSlots.filter(timeSlot => {
                return doctorSchedules.some(schedule => 
                    isTimeSlotWithinSchedule(timeSlot, schedule.startTime, schedule.endTime)
                );
            });
            
            console.log('Available time slots:', availableTimeSlots);
            return availableTimeSlots;
            
        } catch (error) {
            console.error('Error fetching available time slots:', error);
            return [];
        }
    }

    async function renderTimeSlots(grid, onSelect, date) {
        try {
            grid.innerHTML = '<div class="loader">Đang tải các khung giờ trống...</div>';
            
            const allTimeSlots = await fetchAvailableTimeSlots(date);
            
            // Filter out past time slots
            const timeSlots = filterFutureTimeSlots(allTimeSlots, date);
            
            grid.innerHTML = '';
            
            if (timeSlots.length === 0) {
                if (allTimeSlots.length === 0) {
                    grid.innerHTML = '<div class="no-time-slots">Không có khung giờ trống cho ngày này.<br>Vui lòng chọn ngày khác.</div>';
                } else {
                    grid.innerHTML = '<div class="no-time-slots">Không còn khung giờ trống cho hôm nay.<br>Vui lòng chọn ngày sau.</div>';
                }
                return;
            }
            
            timeSlots.forEach(time => {
                const btn = document.createElement('button');
                btn.className = 'time-slot-btn';
                btn.textContent = time;
                btn.onclick = () => onSelect(time, btn);
                grid.appendChild(btn);
            });
            
        } catch (error) {
            console.error('Error rendering time slots:', error);
            grid.innerHTML = '<div class="error-message">Lỗi tải khung giờ. Vui lòng thử lại.<br>Chi tiết: ' + error.message + '</div>';
        }
    }

    calendarDate.addEventListener('change', async function() {
        try {
            const selectedDate = calendarDate.value;
            
            console.log('Calendar date changed to:', selectedDate);
            
            // Reset previous selections
            selectedTimeSlot = null;
            selectedDoctor = null;
            if (selectedTimeBtn) {
                selectedTimeBtn.classList.remove('selected');
                selectedTimeBtn = null;
            }
            
            if (!selectedDate) {
                timeSlotGrid.innerHTML = '';
                availableDoctorsSection.style.display = 'none';
                confirmTimeBookingBtn.style.display = 'none';
                return;
            }
            
            // Check if selected date is in the past
            const today = new Date();
            const selected = new Date(selectedDate);
            today.setHours(0, 0, 0, 0);
            selected.setHours(0, 0, 0, 0);
            
            if (selected < today) {
                timeSlotGrid.innerHTML = '<div class="no-time-slots">Không thể đặt lịch hẹn cho những ngày đã qua.</div>';
                availableDoctorsSection.style.display = 'none';
                confirmTimeBookingBtn.style.display = 'none';
                return;
            }
            
            await renderTimeSlots(timeSlotGrid, (time, btn) => {
                if (selectedTimeBtn) selectedTimeBtn.classList.remove('selected');
                btn.classList.add('selected');
                selectedTimeBtn = btn;
                selectedTimeSlot = time;
                fetchAvailableDoctors(selectedDate, time);
            }, selectedDate);
            
            availableDoctorsSection.style.display = 'none';
            confirmTimeBookingBtn.style.display = 'none';
            
        } catch (error) {
            console.error('Error handling calendar date change:', error);
            timeSlotGrid.innerHTML = '<div class="error-message">Lỗi xử lý ngày đã chọn. Vui lòng thử lại.</div>';
            availableDoctorsSection.style.display = 'none';
            confirmTimeBookingBtn.style.display = 'none';
        }
    });

    function fetchAvailableDoctors(date, time) {
        if (!date || !time) return;
        
        const token = localStorage.getItem('token');
        const formattedTime = time.includes(':') ? `${time}:00` : time;
        
        console.log('Fetching doctors for date:', date, 'time:', formattedTime);
        
        fetch(`https://localhost:7009/api/Doctor/GetDoctorsByDateAndTime?Date=${date}&Time=${encodeURIComponent(formattedTime)}`, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'accept': '*/*'
            }
        })
        .then(response => {
            if (response.status === 404) {
                console.log('No doctors available for this time slot');
                doctorList.innerHTML = '<div class="no-doctors">Không có bác sĩ nào rảnh trong khung giờ này.</div>';
                availableDoctorsSection.style.display = 'block';
                confirmTimeBookingBtn.style.display = 'none';
                return [];
            }
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(doctors => {
            console.log('Available doctors:', doctors);
            
            doctorList.innerHTML = '';
            
            if (!doctors || doctors.length === 0) {
                doctorList.innerHTML = '<div class="no-doctors">No doctors available for this time slot.</div>';
                availableDoctorsSection.style.display = 'block';
                confirmTimeBookingBtn.style.display = 'none';
                return;
            }
            
            doctors.forEach(doctor => {
                const card = document.createElement('div');
                card.className = 'doctor-card';
                card.innerHTML = `
                    <div class="doctor-info">
                        <h4>${doctor.account.fullname}</h4>
                        <p class="doctor-degree">${doctor.degree}</p>
                        <p class="doctor-bio">${doctor.bio || 'Experienced doctor specializing in HIV care'}</p>
                        <div class="doctor-details">
                            <small>Email: ${doctor.account.email}</small>
                            <small>DOB: ${doctor.account.dob}</small>
                            <small>Gender: ${doctor.account.gender ? 'Male' : 'Female'}</small>
                        </div>
                    </div>
                `;
                
                card.onclick = () => {
                    Array.from(doctorList.children).forEach(c => c.classList.remove('selected'));
                    card.classList.add('selected');
                    
                    selectedDoctor = {
                        doctorId: doctor.doctorId,
                        email: doctor.account.email,
                        fullname: doctor.account.fullname,
                        degree: doctor.degree,
                        bio: doctor.bio,
                        dob: doctor.account.dob,
                        gender: doctor.account.gender
                    };
                    confirmTimeBookingBtn.style.display = 'inline-block';
                    
                    console.log('Selected doctor:', selectedDoctor);
                };
                
                doctorList.appendChild(card);
            });
            
            availableDoctorsSection.style.display = 'block';
            confirmTimeBookingBtn.style.display = 'none';
        })
        .catch(error => {
            console.error('Error fetching doctors:', error);
            doctorList.innerHTML = '<div class="error-message">Lỗi tải danh sách bác sĩ. Vui lòng thử lại.</div>';
            availableDoctorsSection.style.display = 'block';
            confirmTimeBookingBtn.style.display = 'none';
        });
    }

    // --- Order by Doctor Section ---
    function fetchDoctors() {
        const token = localStorage.getItem('token');
        
        if (!token) {
            console.error('No token found');
            return;
        }
        
        console.log('Fetching doctors...');
        
        fetch('https://localhost:7009/api/Doctor/GetAllDoctors', {
            headers: {
                'Authorization': `Bearer ${token}`,
                'accept': '*/*'
            }
        })
        .then(response => {
            console.log('Response status:', response.status);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Doctors data:', data);
            
            // Clear existing options except the first one
            doctorSelect.innerHTML = '<option value="">-- Select Doctor --</option>';
            
            if (!data || data.length === 0) {
                console.log('No doctors found');
                const noDataOption = document.createElement('option');
                noDataOption.textContent = 'Không có bác sĩ nào';
                noDataOption.disabled = true;
                doctorSelect.appendChild(noDataOption);
                return;
            }
            
            data.forEach(doctor => {
                console.log('Processing doctor:', doctor);
                const option = document.createElement('option');
                option.value = doctor.account.email;
                option.textContent = `${doctor.account.fullname} (${doctor.degree})`;
                option.doctorData = doctor;
                doctorSelect.appendChild(option);
            });
            
            console.log('Doctors loaded successfully');
        })
        .catch(error => {
            console.error('Error fetching doctors:', error);
            const errorOption = document.createElement('option');
            errorOption.textContent = 'Lỗi tải danh sách bác sĩ';
            errorOption.disabled = true;
            doctorSelect.appendChild(errorOption);
        });
    }

    // Doctor selection change event
    doctorSelect.addEventListener('change', function() {
        const selectedEmail = doctorSelect.value;
        const doctor = Array.from(doctorSelect.options)
            .find(opt => opt.value === selectedEmail)?.doctorData;
        
        if (doctor) {
            console.log('Selected doctor:', doctor);
            selectedDoctorId = doctor.doctorId;
            
            // Clear previous selections
            doctorTimeSlotGrid.innerHTML = '';
            confirmDoctorBookingBtn.style.display = 'none';
            selectedDoctorTimeSlot = null;
            
            // Show schedule section
            doctorScheduleSection.style.display = 'block';
            
            // If doctor has workSchedule, display calendar
            if (doctor.workSchedule && doctor.workSchedule.length > 0) {
                displayDoctorScheduleCalendar(doctor.workSchedule);
            }
        } else {
            doctorScheduleSection.style.display = 'none';
            selectedDoctorId = null;
        }
    });

    // Date selection for doctor schedule
    doctorScheduleDate.addEventListener('change', function() {
        if (!selectedDoctorId || !doctorScheduleDate.value) {
            doctorTimeSlotGrid.innerHTML = '';
            confirmDoctorBookingBtn.style.display = 'none';
            return;
        }
        
        const selectedDate = doctorScheduleDate.value; // This is already in YYYY-MM-DD format
        
        // ✨ FIX: Add debug logging
        console.log('Date input changed to:', selectedDate);
        console.log('Input element value:', doctorScheduleDate.value);
        
        // Check if selected date is in the past
        const today = new Date();
        const selected = new Date(selectedDate + 'T00:00:00'); // ✨ FIX: Add time to avoid timezone issues
        today.setHours(0, 0, 0, 0);
        
        if (selected < today) {
            doctorTimeSlotGrid.innerHTML = '<div class="no-doctors">Không thể đặt lịch hẹn cho những ngày đã qua.</div>';
            confirmDoctorBookingBtn.style.display = 'none';
            return;
        }
        
        selectedDoctorTimeSlot = null;
        fetchDoctorSchedule(selectedDoctorId, selectedDate);
    });

    function fetchDoctorSchedule(doctorId, date) {
        const token = localStorage.getItem('token');
        
        // ✨ FIX: Add debug logging to trace the date issue
        console.log('fetchDoctorSchedule called with:', { doctorId, date });
        console.log('Date input value:', document.getElementById('doctor-schedule-date').value);
        
        fetch(`https://localhost:7009/api/DoctorWorkSchedule/GetDoctorWorkSchedulesByDoctorId/${doctorId}`, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'accept': '*/*'
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(schedules => {
            console.log('Fetched schedules:', schedules);
            console.log('Looking for schedule on date:', date); // Debug log
            
            // Find ALL schedules for the selected date (not just the first one)
            const daySchedules = schedules.filter(s => {
                console.log('Comparing:', s.workDate, 'with', date); // Debug log
                return s.workDate === date && s.isAvailable;
            });
            
            console.log('Found schedules for the day:', daySchedules); // Debug log
            
            doctorTimeSlotGrid.innerHTML = '';
            
            if (!daySchedules || daySchedules.length === 0) {
                doctorTimeSlotGrid.innerHTML = '<div class="no-doctors">Không có lịch làm việc cho ngày này.</div>';
                confirmDoctorBookingBtn.style.display = 'none';
                return;
            }
            
            // Combine all time slots from all schedules for this day
            let allDayTimeSlots = [];
            
            daySchedules.forEach(schedule => {
                console.log(`Processing schedule: ${schedule.startTime} - ${schedule.endTime}`);
                const startTime = schedule.startTime.substring(0, 5);
                const endTime = schedule.endTime.substring(0, 5);
                const scheduleTimeSlots = generateTimeSlotsBetween(startTime, endTime);
                allDayTimeSlots = allDayTimeSlots.concat(scheduleTimeSlots);
            });
            
            console.log('All time slots for the day:', allDayTimeSlots);
            
            // Remove duplicates and sort
            allDayTimeSlots = [...new Set(allDayTimeSlots)].sort();
            
            // ✨ FIX: Use the exact date passed to function, not the input value
            const timeSlots = filterFutureTimeSlots(allDayTimeSlots, date);
            
            console.log('Available time slots after filtering:', timeSlots);
            
            if (timeSlots.length === 0) {
                if (allDayTimeSlots.length === 0) {
                    doctorTimeSlotGrid.innerHTML = '<div class="no-doctors">Không có khung giờ trống cho ngày này.</div>';
                } else {
                    doctorTimeSlotGrid.innerHTML = '<div class="no-doctors">Không còn khung giờ trống cho hôm nay.</div>';
                }
                confirmDoctorBookingBtn.style.display = 'none';
                return;
            }
            
            timeSlots.forEach(time => {
                const btn = document.createElement('button');
                btn.className = 'time-slot-btn';
                btn.textContent = time;
                btn.onclick = () => {
                    Array.from(doctorTimeSlotGrid.children).forEach(b => b.classList.remove('selected'));
                    btn.classList.add('selected');
                    selectedDoctorTimeSlot = time;
                    confirmDoctorBookingBtn.style.display = 'inline-block';
                };
                doctorTimeSlotGrid.appendChild(btn);
            });
            
            confirmDoctorBookingBtn.style.display = 'none';
        })
        .catch(error => {
            console.error('Error fetching doctor schedule:', error);
            doctorTimeSlotGrid.innerHTML = '<div class="error-message">Lỗi tải lịch làm việc. Vui lòng thử lại.</div>';
            confirmDoctorBookingBtn.style.display = 'none';
        });
    }

    // Add global variables for calendar navigation
    let currentCalendarMonth = new Date().getMonth();
    let currentCalendarYear = new Date().getFullYear();

    function displayDoctorScheduleCalendar(schedules) {
        const existingCalendar = document.querySelector('.doctor-schedule-calendar');
        if (existingCalendar) {
            existingCalendar.remove();
        }
        
        const calendarContainer = document.createElement('div');
        calendarContainer.className = 'doctor-schedule-calendar';
        calendarContainer.innerHTML = `
            <h4>Lịch làm việc của bác sĩ</h4>
            <div class="calendar-navigation">
                <button id="prev-month-btn" class="nav-btn">
                    <i class="fas fa-chevron-left"></i>
                </button>
                <div id="current-month-year" class="month-year-display">
                    ${getMonthYearDisplay(currentCalendarMonth, currentCalendarYear)}
                </div>
                <button id="next-month-btn" class="nav-btn">
                    <i class="fas fa-chevron-right"></i>
                </button>
            </div>
            <div class="calendar-grid">
                <div class="calendar-header">
                    <div class="day-header">Sun</div>
                    <div class="day-header">Mon</div>
                    <div class="day-header">Tue</div>
                    <div class="day-header">Wed</div>
                    <div class="day-header">Thu</div>
                    <div class="day-header">Fri</div>
                    <div class="day-header">Sat</div>
                </div>
                <div class="calendar-body" id="calendar-body"></div>
            </div>
        `;

        const dateInput = document.getElementById('doctor-schedule-date');
        dateInput.parentNode.insertBefore(calendarContainer, dateInput);

        // Store schedules globally for navigation
        window.doctorSchedules = schedules;

        const schedulesByDate = {};
        schedules.forEach(schedule => {
            const date = schedule.workDate;
            if (!schedulesByDate[date]) {
                schedulesByDate[date] = [];
            }
            schedulesByDate[date].push(schedule);
        });

        generateCalendarMonth(schedulesByDate);
        
        // Add event listeners for navigation
        setupCalendarNavigation();
    }

    function setupCalendarNavigation() {
        const prevBtn = document.getElementById('prev-month-btn');
        const nextBtn = document.getElementById('next-month-btn');
        
        if (prevBtn) {
            prevBtn.addEventListener('click', () => {
                currentCalendarMonth--;
                if (currentCalendarMonth < 0) {
                    currentCalendarMonth = 11;
                    currentCalendarYear--;
                }
                updateCalendarDisplay();
            });
        }
        
        if (nextBtn) {
            nextBtn.addEventListener('click', () => {
                currentCalendarMonth++;
                if (currentCalendarMonth > 11) {
                    currentCalendarMonth = 0;
                    currentCalendarYear++;
                }
                updateCalendarDisplay();
            });
        }
    }

    function updateCalendarDisplay() {
        // Update month/year display
        const monthYearDisplay = document.getElementById('current-month-year');
        if (monthYearDisplay) {
            monthYearDisplay.textContent = getMonthYearDisplay(currentCalendarMonth, currentCalendarYear);
        }
        
        // Regenerate calendar with current schedules
        if (window.doctorSchedules) {
            const schedulesByDate = {};
            window.doctorSchedules.forEach(schedule => {
                const date = schedule.workDate;
                if (!schedulesByDate[date]) {
                    schedulesByDate[date] = [];
                }
                schedulesByDate[date].push(schedule);
            });
            
            generateCalendarMonth(schedulesByDate);
        }
    }

    function getMonthYearDisplay(month, year) {
        const monthNames = [
            'Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6',
            'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'
        ];
        return `${monthNames[month]} ${year}`;
    }

    function generateCalendarMonth(schedulesByDate) {
        const calendarBody = document.getElementById('calendar-body');
        if (!calendarBody) return;
        
        // ✨ FIX: Use current calendar month/year instead of today's
        const firstDay = new Date(currentCalendarYear, currentCalendarMonth, 1);
        const lastDay = new Date(currentCalendarYear, currentCalendarMonth + 1, 0);
        const daysInMonth = lastDay.getDate();
        const startingDayOfWeek = firstDay.getDay();

        calendarBody.innerHTML = '';

        // Add empty cells for days before the first day of the month
        for (let i = 0; i < startingDayOfWeek; i++) {
            const emptyCell = document.createElement('div');
            emptyCell.className = 'calendar-day empty';
            calendarBody.appendChild(emptyCell);
        }

        // Add days of the month
        for (let day = 1; day <= daysInMonth; day++) {
            const dayCell = document.createElement('div');
            dayCell.className = 'calendar-day';
            
            // ✨ FIX: Use current calendar month/year
            const dateStr = `${currentCalendarYear}-${String(currentCalendarMonth + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            
            dayCell.innerHTML = `<div class="day-number">${day}</div>`;
            
            // ✨ FIX: Check if date is in the past and disable it
            const cellDate = new Date(currentCalendarYear, currentCalendarMonth, day);
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            
            if (cellDate < today) {
                dayCell.classList.add('past-date');
            }
            
            if (schedulesByDate[dateStr]) {
                dayCell.classList.add('has-schedule');
                const timesDiv = document.createElement('div');
                timesDiv.className = 'schedule-times';
                
                schedulesByDate[dateStr].forEach(schedule => {
                    const timeSlot = document.createElement('div');
                    timeSlot.className = `time-slot ${schedule.isAvailable ? 'available' : 'unavailable'}`;
                    timeSlot.textContent = `${schedule.startTime.substring(0,5)}-${schedule.endTime.substring(0,5)}`;
                    
                    // Only allow click if date is not in past and schedule is available
                    if (schedule.isAvailable && cellDate >= today) {
                        timeSlot.onclick = () => {
                            console.log('Setting date to:', dateStr);
                            document.getElementById('doctor-schedule-date').value = dateStr;
                            
                            const changeEvent = new Event('change', { bubbles: true });
                            document.getElementById('doctor-schedule-date').dispatchEvent(changeEvent);
                        };
                    } else if (cellDate < today) {
                        timeSlot.classList.add('past-slot');
                    }
                    
                    timesDiv.appendChild(timeSlot);
                });
                
                dayCell.appendChild(timesDiv);
            }
            
            calendarBody.appendChild(dayCell);
        }
    }

    // Modal Functions
    function showConfirmationModal(bookingData) {
        const modal = document.createElement('div');
        modal.className = 'modal-overlay';
        modal.innerHTML = `
            <div class="confirmation-modal">
                <div class="modal-header">
                    <h3><i class="fas fa-calendar-check"></i> Xác nhận lịch hẹn</h3>
                    <button class="modal-close" onclick="closeModal()">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="booking-summary">
                        <h4><i class="fas fa-clipboard-list"></i> Thông tin lịch hẹn</h4>
                        <div class="summary-item">
                            <span class="summary-label"><i class="fas fa-calendar"></i> Ngày:</span>
                            <span class="summary-value">${formatDate(bookingData.appointmentDate)}</span>
                        </div>
                        <div class="summary-item">
                            <span class="summary-label"><i class="fas fa-clock"></i> Thời gian:</span>
                            <span class="summary-value">${bookingData.appointmentTime}</span>
                        </div>
                        <div class="summary-item">
                            <span class="summary-label"><i class="fas fa-stethoscope"></i> Loại:</span>
                            <span class="summary-value">Tư vấn điều trị HIV</span>
                        </div>
                    </div>
                    
                    <div class="doctor-summary">
                        <h5><i class="fas fa-user-md"></i> Thông tin bác sĩ</h5>
                        <p><strong>Tên:</strong> ${bookingData.doctorName}</p>
                        <p><strong>Email:</strong> ${bookingData.doctorEmail}</p>
                        <p><strong>Chuyên môn:</strong> ${bookingData.degree || 'Điều trị HIV/AIDS'}</p>
                    </div>
                    
                    <div class="modal-actions">
                        <button class="modal-btn modal-btn-cancel" onclick="closeModal()">
                            <i class="fas fa-times"></i> Hủy bỏ
                        </button>
                        <button class="modal-btn modal-btn-confirm" onclick="confirmBooking()">
                            <i class="fas fa-check"></i> Xác nhận đặt lịch
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        window.currentBookingData = bookingData;
        document.body.style.overflow = 'hidden';
        
        modal.addEventListener('click', function(e) {
            if (e.target === modal) {
                closeModal();
            }
        });
    }

    function closeModal() {
        const modal = document.querySelector('.modal-overlay');
        if (modal) {
            modal.remove();
        }
        document.body.style.overflow = 'auto';
        window.currentBookingData = null;
    }

    function formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }

    function confirmBooking() {
        const confirmBtn = document.querySelector('.modal-btn-confirm');
        const cancelBtn = document.querySelector('.modal-btn-cancel');
        
        confirmBtn.disabled = true;
        confirmBtn.innerHTML = '<span class="loading-spinner"></span>Đang tạo lịch hẹn...';
        cancelBtn.disabled = true;
        
        createAppointment(window.currentBookingData)
            .then(response => {
                console.log('Appointment created successfully:', response);
                showSuccessMessage();
                
                setTimeout(() => {
                    closeModal();
                    window.location.href = '../appointment-view/view-appointment.html';
                }, 2000);
            })
            .catch(error => {
                console.error('Error creating appointment:', error);
                showErrorMessage(error.message);
                
                confirmBtn.disabled = false;
                confirmBtn.innerHTML = '<i class="fas fa-check"></i> Confirm Booking';
                cancelBtn.disabled = false;
            });
    }

    function showSuccessMessage() {
        const modalBody = document.querySelector('.modal-body');
        modalBody.innerHTML = `
            <div class="success-message">
                <div class="success-icon">
                    <i class="fas fa-check-circle"></i>
                </div>
                <h4>Đã tạo lịch hẹn thành công!</h4>
                <p>Bạn sẽ nhận được email xác nhận trong thời gian ngắn.</p>
                <p>Đang chuyển đến trang lịch hẹn của bạn...</p>
            </div>
        `;
    }

    function showErrorMessage(errorMessage) {
        const modalBody = document.querySelector('.modal-body');
        modalBody.innerHTML = `
            <div class="error-message">
                <div style="text-align: center; padding: 30px;">
                    <div style="font-size: 48px; color: #e74c3c; margin-bottom: 15px;">
                        <i class="fas fa-exclamation-triangle"></i>
                    </div>
                    <h4 style="color: #e74c3c; margin: 0 0 10px 0;">Đặt lịch thất bại</h4>
                    <p style="color: #6c757d; margin: 0 0 20px 0;">${errorMessage}</p>
                    <button class="modal-btn modal-btn-confirm" onclick="closeModal()">
                        <i class="fas fa-arrow-left"></i> Thử lại
                    </button>
                </div>
            </div>
        `;
    }

    function createAppointment(appointmentData) {
        const token = localStorage.getItem('token');
        
        let formattedTime = appointmentData.appointmentTime;
        if (formattedTime && formattedTime.split(':').length === 2) {
            formattedTime = `${formattedTime}:00`;
        }
        
        const requestBody = {
            doctorId: appointmentData.doctorId,
            appointmentDate: appointmentData.appointmentDate,
            appointmentTime: formattedTime,
            notes: appointmentData.notes || ''
        };
        
        return fetch('https://localhost:7009/api/Appointment/CreateAppointment', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
                'accept': '*/*'
            },
            body: JSON.stringify(requestBody)
        })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(`HTTP error! status: ${response.status}, message: ${text}`);
                });
            }
            return response.json();
        });
    }

    // Event Listeners for Booking Buttons
    confirmTimeBookingBtn.addEventListener('click', function() {
        if (!calendarDate.value || !selectedTimeSlot || !selectedDoctor) {
            alert('Vui lòng chọn đầy đủ thông tin.');
            return;
        }
        
        const selectedDate = new Date(calendarDate.value);
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        
        if (selectedDate < today) {
            alert('Please select a future date.');
            return;
        }
        
        const bookingData = {
            doctorId: selectedDoctor.doctorId,
            doctorEmail: selectedDoctor.email,
            doctorName: selectedDoctor.fullname,
            appointmentDate: calendarDate.value,
            appointmentTime: selectedTimeSlot,
            degree: selectedDoctor.degree,
            notes: `Cuộc hẹn với Bác sĩ ${selectedDoctor.fullname} (${selectedDoctor.degree})`
        };
        
        showConfirmationModal(bookingData);
    });

    confirmDoctorBookingBtn.addEventListener('click', function() {
        if (!selectedDoctorId || !doctorScheduleDate.value || !selectedDoctorTimeSlot) {
            alert('Please select all fields.');
            return;
        }
        
        const selectedDate = new Date(doctorScheduleDate.value);
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        
        if (selectedDate < today) {
            alert('Please select a future date.');
            return;
        }
        
        const selectedEmail = doctorSelect.value;
        const doctor = Array.from(doctorSelect.options)
            .find(opt => opt.value === selectedEmail)?.doctorData;
        
        if (!doctor) {
            alert('Không tìm thấy thông tin bác sĩ.');
            return;
        }
        
        const bookingData = {
            doctorId: selectedDoctorId,
            doctorEmail: doctor.account.email,
            doctorName: doctor.account.fullname,
            appointmentDate: doctorScheduleDate.value,
            appointmentTime: selectedDoctorTimeSlot,
            degree: doctor.degree,
            notes: `Appointment with Dr. ${doctor.account.fullname} (${doctor.degree})`
        };
        
        showConfirmationModal(bookingData);
    });

    // Make functions global for modal
    window.closeModal = closeModal;
    window.confirmBooking = confirmBooking;

    // Initialize
    fetchDoctors();

    // Set minimum date for date inputs to prevent past date selection
    function setMinDateForInputs() {
        const today = new Date();
        const todayString = today.toISOString().split('T')[0];
        
        if (calendarDate) {
            calendarDate.min = todayString;
        }
        
        if (doctorScheduleDate) {
            doctorScheduleDate.min = todayString;
        }
    }

    // Call this function when the page loads
    setMinDateForInputs();

    // Utility function to check if time slot is in the past
    function isTimeSlotInPast(date, time) {
        const now = new Date();
        const slotDateTime = new Date(date + 'T' + time + ':00');
        return slotDateTime < now;
    }

    // Utility function to filter out past time slots
    function filterFutureTimeSlots(timeSlots, date) {
        const now = new Date();
        const selectedDate = new Date(date);
        
        // If selected date is not today, return all slots
        if (selectedDate.toDateString() !== now.toDateString()) {
            return timeSlots;
        }
        
        // Filter out past time slots for today
        return timeSlots.filter(time => {
            const [hour, minute] = time.split(':').map(Number);
            const slotTime = new Date();
            slotTime.setHours(hour, minute, 0, 0);
            return slotTime > now;
        });
    }

    // Utility function to generate time slots between start and end time
    function generateTimeSlotsBetween(startTime, endTime) {
        const timeSlots = [];
        
        try {
            const [startHour, startMinute] = startTime.split(':').map(Number);
            const [endHour, endMinute] = endTime.split(':').map(Number);
            
            const startMinutes = startHour * 60 + startMinute;
            const endMinutes = endHour * 60 + endMinute;
            
            // Generate 30-minute time slots
            const slotDuration = 30;
            
            for (let minutes = startMinutes; minutes < endMinutes; minutes += slotDuration) {
                const hours = Math.floor(minutes / 60);
                const mins = minutes % 60;
                
                // Format time as HH:MM
                const timeSlot = `${String(hours).padStart(2, '0')}:${String(mins).padStart(2, '0')}`;
                timeSlots.push(timeSlot);
            }
            
            console.log(`Generated time slots between ${startTime} and ${endTime}:`, timeSlots);
            return timeSlots;
            
        } catch (error) {
            console.error('Error generating time slots:', error);
            return [];
        }
    }

    // Utility function to check if a time slot falls within a doctor's schedule
    function isTimeSlotWithinSchedule(timeSlot, startTime, endTime) {
        try {
            const [slotHour, slotMin] = timeSlot.split(':').map(Number);
            const [startHour, startMin] = startTime.split(':').map(Number);
            const [endHour, endMin] = endTime.split(':').map(Number);
            
            const slotMinutes = slotHour * 60 + slotMin;
            const startMinutes = startHour * 60 + startMin;
            const endMinutes = endHour * 60 + endMin;
            
            return slotMinutes >= startMinutes && slotMinutes < endMinutes;
        } catch (error) {
            console.error('Error checking time slot within schedule:', error);
            return false;
        }
    }
});