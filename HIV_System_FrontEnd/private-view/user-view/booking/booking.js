document.addEventListener('DOMContentLoaded', function() {
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
            
            // Generate all possible time slots for the day (8:00 AM to 6:00 PM)
            const dayStartTime = '08:00';
            const dayEndTime = '18:00';
            const allPossibleTimeSlots = generateTimeSlotsBetween(dayStartTime, dayEndTime);
            
            console.log('All possible time slots for the day:', allPossibleTimeSlots);
            
            // Get all doctors to check their schedules
            const doctorsResponse = await fetch('https://localhost:7009/api/Doctor/GetAllDoctors', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'accept': '*/*'
                }
            });
            
            if (!doctorsResponse.ok) {
                throw new Error(`HTTP error! status: ${doctorsResponse.status}`);
            }
            
            const doctors = await doctorsResponse.json();
            console.log('All doctors:', doctors);
            
            if (!doctors || doctors.length === 0) {
                return [];
            }
            
            // Fetch all doctor schedules for the selected date (optimize by fetching once per doctor)
            const doctorSchedules = [];
            
            for (const doctor of doctors) {
                try {
                    const scheduleResponse = await fetch(`https://localhost:7009/api/DoctorWorkSchedule/GetDoctorWorkSchedulesByDoctorId/${doctor.doctorId}`, {
                        headers: {
                            'Authorization': `Bearer ${token}`,
                            'accept': '*/*'
                        }
                    });
                    
                    if (scheduleResponse.ok) {
                        const schedules = await scheduleResponse.json();
                        const schedule = schedules.find(s => s.workDate === date);
                        
                        if (schedule && schedule.isAvailable) {
                            doctorSchedules.push({
                                doctorId: doctor.doctorId,
                                startTime: schedule.startTime.substring(0, 5),
                                endTime: schedule.endTime.substring(0, 5)
                            });
                        }
                    }
                } catch (error) {
                    console.warn(`Error fetching schedule for doctor ${doctor.doctorId}:`, error);
                }
            }
            
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
        grid.innerHTML = '<div class="loader">Loading available time slots...</div>';
        
        const allTimeSlots = await fetchAvailableTimeSlots(date);
        
        // Filter out past time slots
        const timeSlots = filterFutureTimeSlots(allTimeSlots, date);
        
        grid.innerHTML = '';
        
        if (timeSlots.length === 0) {
            if (allTimeSlots.length === 0) {
                grid.innerHTML = '<div class="no-time-slots">No available time slots for this date.</div>';
            } else {
                grid.innerHTML = '<div class="no-time-slots">No available time slots remaining for today.</div>';
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
    }

    calendarDate.addEventListener('change', async function() {
        const selectedDate = calendarDate.value;
        
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
            timeSlotGrid.innerHTML = '<div class="no-time-slots">Cannot book appointments for past dates.</div>';
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
        selectedTimeSlot = null;
        selectedDoctor = null;
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
                doctorList.innerHTML = '<div class="no-doctors">No doctors available for this time slot.</div>';
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
            doctorList.innerHTML = '<div class="error-message">Error loading doctors. Please try again.</div>';
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
                noDataOption.textContent = 'No doctors available';
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
            errorOption.textContent = 'Error loading doctors';
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
        
        const selectedDate = doctorScheduleDate.value;
        
        // Check if selected date is in the past
        const today = new Date();
        const selected = new Date(selectedDate);
        today.setHours(0, 0, 0, 0);
        selected.setHours(0, 0, 0, 0);
        
        if (selected < today) {
            doctorTimeSlotGrid.innerHTML = '<div class="no-doctors">Cannot book appointments for past dates.</div>';
            confirmDoctorBookingBtn.style.display = 'none';
            return;
        }
        
        selectedDoctorTimeSlot = null;
        fetchDoctorSchedule(selectedDoctorId, selectedDate);
    });

    function fetchDoctorSchedule(doctorId, date) {
        const token = localStorage.getItem('token');
        
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
            
            // Find the schedule for the selected date
            const schedule = schedules.find(s => s.workDate === date);
            
            doctorTimeSlotGrid.innerHTML = '';
            
            if (!schedule) {
                doctorTimeSlotGrid.innerHTML = '<div class="no-doctors">No schedule available for this date.</div>';
                confirmDoctorBookingBtn.style.display = 'none';
                return;
            }
            
            if (!schedule.isAvailable) {
                doctorTimeSlotGrid.innerHTML = '<div class="no-doctors">Doctor is not available on this date.</div>';
                confirmDoctorBookingBtn.style.display = 'none';
                return;
            }
            
            // Generate time slots
            const startTime = schedule.startTime.substring(0, 5);
            const endTime = schedule.endTime.substring(0, 5);
            const allTimeSlots = generateTimeSlotsBetween(startTime, endTime);
            
            // Filter out past time slots
            const timeSlots = filterFutureTimeSlots(allTimeSlots, date);
            
            if (timeSlots.length === 0) {
                if (allTimeSlots.length === 0) {
                    doctorTimeSlotGrid.innerHTML = '<div class="no-doctors">No available time slots for this date.</div>';
                } else {
                    doctorTimeSlotGrid.innerHTML = '<div class="no-doctors">No available time slots remaining for today.</div>';
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
            doctorTimeSlotGrid.innerHTML = '<div class="error-message">Error fetching schedule. Please try again.</div>';
            confirmDoctorBookingBtn.style.display = 'none';
        });
    }

    // Utility Functions
    function generateTimeSlotsBetween(startTime, endTime, intervalMinutes = 30) {
        const slots = [];
        
        try {
            const [startHour, startMin] = startTime.split(':').map(Number);
            const [endHour, endMin] = endTime.split(':').map(Number);
            
            // Validate time inputs
            if (startHour < 0 || startHour > 23 || startMin < 0 || startMin > 59 ||
                endHour < 0 || endHour > 23 || endMin < 0 || endMin > 59) {
                console.error('Invalid time format:', startTime, endTime);
                return slots;
            }
            
            const start = new Date();
            start.setHours(startHour, startMin, 0, 0);
            
            const end = new Date();
            end.setHours(endHour, endMin, 0, 0);
            
            // Ensure end time is after start time
            if (end <= start) {
                console.error('End time must be after start time:', startTime, endTime);
                return slots;
            }
            
            const current = new Date(start);
            
            while (current < end) {
                const timeStr = current.toTimeString().substring(0, 5);
                slots.push(timeStr);
                current.setMinutes(current.getMinutes() + intervalMinutes);
            }
            
            return slots;
        } catch (error) {
            console.error('Error generating time slots:', error);
            return slots;
        }
    }

    function displayDoctorScheduleCalendar(schedules) {
        const existingCalendar = document.querySelector('.doctor-schedule-calendar');
        if (existingCalendar) {
            existingCalendar.remove();
        }
        
        const calendarContainer = document.createElement('div');
        calendarContainer.className = 'doctor-schedule-calendar';
        calendarContainer.innerHTML = `
            <h4>Doctor's Work Schedule</h4>
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

        const schedulesByDate = {};
        schedules.forEach(schedule => {
            const date = schedule.workDate;
            if (!schedulesByDate[date]) {
                schedulesByDate[date] = [];
            }
            schedulesByDate[date].push(schedule);
        });

        generateCalendarMonth(schedulesByDate);
    }

    function generateCalendarMonth(schedulesByDate) {
        const calendarBody = document.getElementById('calendar-body');
        if (!calendarBody) return;
        
        const today = new Date();
        const currentMonth = today.getMonth();
        const currentYear = today.getFullYear();

        const firstDay = new Date(currentYear, currentMonth, 1);
        const lastDay = new Date(currentYear, currentMonth + 1, 0);
        const daysInMonth = lastDay.getDate();
        const startingDayOfWeek = firstDay.getDay();

        calendarBody.innerHTML = '';

        for (let i = 0; i < startingDayOfWeek; i++) {
            const emptyCell = document.createElement('div');
            emptyCell.className = 'calendar-day empty';
            calendarBody.appendChild(emptyCell);
        }

        for (let day = 1; day <= daysInMonth; day++) {
            const dayCell = document.createElement('div');
            dayCell.className = 'calendar-day';
            
            const date = new Date(currentYear, currentMonth, day);
            const dateStr = date.toISOString().split('T')[0];
            
            dayCell.innerHTML = `<div class="day-number">${day}</div>`;
            
            if (schedulesByDate[dateStr]) {
                dayCell.classList.add('has-schedule');
                const timesDiv = document.createElement('div');
                timesDiv.className = 'schedule-times';
                
                schedulesByDate[dateStr].forEach(schedule => {
                    const timeSlot = document.createElement('div');
                    timeSlot.className = `time-slot ${schedule.isAvailable ? 'available' : 'unavailable'}`;
                    timeSlot.textContent = `${schedule.startTime.substring(0,5)}-${schedule.endTime.substring(0,5)}`;
                    if (schedule.isAvailable) {
                        timeSlot.onclick = () => {
                            document.getElementById('doctor-schedule-date').value = dateStr;
                            fetchDoctorSchedule(selectedDoctorId, dateStr);
                        };
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
                    <h3><i class="fas fa-calendar-check"></i> Confirm Appointment</h3>
                    <button class="modal-close" onclick="closeModal()">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="booking-summary">
                        <h4><i class="fas fa-clipboard-list"></i> Booking Summary</h4>
                        <div class="summary-item">
                            <span class="summary-label"><i class="fas fa-calendar"></i> Date:</span>
                            <span class="summary-value">${formatDate(bookingData.appointmentDate)}</span>
                        </div>
                        <div class="summary-item">
                            <span class="summary-label"><i class="fas fa-clock"></i> Time:</span>
                            <span class="summary-value">${bookingData.appointmentTime}</span>
                        </div>
                        <div class="summary-item">
                            <span class="summary-label"><i class="fas fa-stethoscope"></i> Type:</span>
                            <span class="summary-value">HIV Care Consultation</span>
                        </div>
                    </div>
                    
                    <div class="doctor-summary">
                        <h5><i class="fas fa-user-md"></i> Doctor Information</h5>
                        <p><strong>Name:</strong> ${bookingData.doctorName}</p>
                        <p><strong>Email:</strong> ${bookingData.doctorEmail}</p>
                        <p><strong>Specialization:</strong> ${bookingData.degree || 'HIV/AIDS Care'}</p>
                    </div>
                    
                    <div class="modal-actions">
                        <button class="modal-btn modal-btn-cancel" onclick="closeModal()">
                            <i class="fas fa-times"></i> Cancel
                        </button>
                        <button class="modal-btn modal-btn-confirm" onclick="confirmBooking()">
                            <i class="fas fa-check"></i> Confirm Booking
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
        confirmBtn.innerHTML = '<span class="loading-spinner"></span>Creating Appointment...';
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
                <h4>Appointment Created Successfully!</h4>
                <p>You will receive a confirmation email shortly.</p>
                <p>Redirecting to your appointments...</p>
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
                    <h4 style="color: #e74c3c; margin: 0 0 10px 0;">Booking Failed</h4>
                    <p style="color: #6c757d; margin: 0 0 20px 0;">${errorMessage}</p>
                    <button class="modal-btn modal-btn-confirm" onclick="closeModal()">
                        <i class="fas fa-arrow-left"></i> Try Again
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
            alert('Please select all fields.');
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
            notes: `Appointment with Dr. ${selectedDoctor.fullname} (${selectedDoctor.degree})`
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
            alert('Doctor information not found.');
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