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

    // --- Order by Time ---
    const timeSlots = [
        '09:00', '10:00', '11:00', '14:00', '15:00'
    ];
    // Update the time slot rendering function
    function renderTimeSlots(grid, onSelect) {
        grid.innerHTML = '';
        timeSlots.forEach(time => {
            const btn = document.createElement('button');
            btn.className = 'time-slot-btn';
            btn.textContent = time;
            btn.onclick = () => onSelect(time, btn);
            grid.appendChild(btn);
        });
    }
    let selectedTimeBtn = null;
    calendarDate.addEventListener('change', function() {
        renderTimeSlots(timeSlotGrid, (time, btn) => {
            if (selectedTimeBtn) selectedTimeBtn.classList.remove('selected');
            btn.classList.add('selected');
            selectedTimeBtn = btn;
            selectedTimeSlot = time;
            fetchAvailableDoctors(calendarDate.value, time);
        });
        availableDoctorsSection.style.display = 'none';
        confirmTimeBookingBtn.style.display = 'none';
        selectedTimeSlot = null;
        selectedDoctor = null;
    });
    // Replace the fetchAvailableDoctors function with this updated version:
    function fetchAvailableDoctors(date, time) {
        if (!date || !time) return;
        
        const token = localStorage.getItem('token');
        
        // Format the time to match the API expectation (HH:MM:SS format)
        const formattedTime = time.includes(':') ? `${time}:00` : time;
        
        console.log('Fetching doctors for date:', date, 'time:', formattedTime);
        
        fetch(`https://localhost:7009/api/Doctor/GetDoctorByDateAndTime?Date=${date}&Time=${encodeURIComponent(formattedTime)}`, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'accept': '*/*'
            }
        })
        .then(response => {
            // Handle 404 specifically - no doctors available
            if (response.status === 404) {
                console.log('No doctors available for this time slot');
                doctorList.innerHTML = '<div class="no-doctors">No doctors available for this time slot.</div>';
                availableDoctorsSection.style.display = 'block';
                confirmTimeBookingBtn.style.display = 'none';
                return []; // Return empty array
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
            
            // Create doctor cards
            doctors.forEach(doctor => {
                const card = document.createElement('div');
                card.className = 'doctor-card';
                card.innerHTML = `
                    <div class="doctor-info">
                        <h4>${doctor.fullname}</h4>
                        <p class="doctor-degree">${doctor.degree}</p>
                        <p class="doctor-bio">${doctor.bio || 'Experienced doctor specializing in HIV care'}</p>
                        <div class="doctor-details">
                            <small>Email: ${doctor.email}</small>
                            <small>DOB: ${doctor.dob}</small>
                        </div>
                    </div>
                `;
                
                card.onclick = () => {
                    // Remove selection from other cards
                    Array.from(doctorList.children).forEach(c => c.classList.remove('selected'));
                    card.classList.add('selected');
                    
                    // Store selected doctor
                    selectedDoctor = doctor;
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
    confirmTimeBookingBtn.addEventListener('click', function() {
        if (!calendarDate.value || !selectedTimeSlot || !selectedDoctor) {
            return alert('Please select all fields.');
        }
        
        // Create booking data with available information
        const bookingData = {
            doctorEmail: selectedDoctor.email,
            doctorName: selectedDoctor.fullname,
            appointmentDate: calendarDate.value,
            appointmentTime: selectedTimeSlot,
            patientNotes: `Appointment with Dr. ${selectedDoctor.fullname} (${selectedDoctor.degree})`
        };
        
        console.log('Booking data:', bookingData);
        
        // For now, show confirmation (you can implement actual booking API later)
        const confirmMsg = `
            Booking Details:
            Doctor: ${selectedDoctor.fullname} (${selectedDoctor.degree})
            Date: ${calendarDate.value}
            Time: ${selectedTimeSlot}
            Email: ${selectedDoctor.email}
            
            Would you like to confirm this appointment?
        `;
        
        if (confirm(confirmMsg)) {
            // Here you would call your booking API when it's ready
            // bookAppointment(bookingData);
            
            // For now, just show success message
            alert('Appointment request submitted successfully!\nYou will receive a confirmation email shortly.');
            
            // Reset form
            resetBookingForm();
        }
    });

    // --- Order by Doctor ---
    function fetchDoctors() {
        const token = localStorage.getItem('token');
        fetch('https://localhost:7009/api/Doctor/GetAllDoctors', {
            headers: {
                'Authorization': `Bearer ${token}`,
                'accept': '*/*'
            }
        })
        .then(response => response.json())
        .then(data => {
            doctorSelect.options.length = 1; // Keep the first option
            data.forEach(doctor => {
                console.log('Doctor object:', doctor); // Debug log to see all properties
                const option = document.createElement('option');
                option.value = doctor.email;
                option.textContent = `${doctor.fullname} (${doctor.degree})`;
                option.doctorData = doctor; // Attach the whole doctor object
                doctorSelect.appendChild(option);
            });
        })
        .catch(error => {
            console.error('Error fetching doctors:', error);
        });
    }
    doctorSelect.addEventListener('change', function() {
        const selectedEmail = doctorSelect.value;
        const doctor = Array.from(doctorSelect.options)
            .find(opt => opt.value === selectedEmail)?.doctorData;
        if (doctor) {
            console.log('Selected doctor:', doctor); // Debug log
            // Try different possible property names for the doctor ID
            selectedDoctorId = doctor.userId || doctor.doctorId || doctor.id || doctor.ID;
            console.log('Doctor ID:', selectedDoctorId); // Debug log
            
            if (selectedDoctorId) {
                fetchDoctorWorkSchedule(selectedDoctorId);
                doctorScheduleSection.style.display = 'block';
            } else {
                console.error('Could not find doctor ID in:', doctor);
                alert('Doctor ID not found');
            }
        } else {
            doctorScheduleSection.style.display = 'none';
        }
    });
    doctorScheduleDate.addEventListener('change', function() {
        if (!selectedDoctorId || !doctorScheduleDate.value) return;
        fetchDoctorSchedule(selectedDoctorId, doctorScheduleDate.value);
    });
    function fetchDoctorSchedule(doctorId, date) {
        const token = localStorage.getItem('token');
        fetch(`https://localhost:7009/api/DoctorWorkSchedule/GetDoctorWorkSchedulesByDoctorId/${doctorId}`, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'accept': '*/*'
            }
        })
        .then(res => res.json())
        .then(schedules => {
            // Find the schedule for the selected date
            const schedule = schedules.find(s => s.date === date);
            doctorTimeSlotGrid.innerHTML = '';
            if (!schedule || !schedule.timeSlots || schedule.timeSlots.length === 0) {
                doctorTimeSlotGrid.textContent = 'No available time slots for this date.';
                confirmDoctorBookingBtn.style.display = 'none';
                return;
            }
            let selectedBtn = null;
            schedule.timeSlots.forEach(time => {
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
        .catch(err => {
            doctorTimeSlotGrid.textContent = 'Error fetching schedule.';
            confirmDoctorBookingBtn.style.display = 'none';
        });
    }
    confirmDoctorBookingBtn.addEventListener('click', function() {
        if (!selectedDoctorId || !doctorScheduleDate.value || !selectedDoctorTimeSlot) return alert('Please select all fields.');
        // TODO: Implement booking API call here
        alert(`Booked with doctor ID ${selectedDoctorId} on ${doctorScheduleDate.value} at ${selectedDoctorTimeSlot}`);
    });

    // Update the fetchDoctorWorkSchedule function to use doctor ID directly:
    function fetchDoctorWorkSchedule(doctorId) {
        const token = localStorage.getItem('token');
        
        console.log('Fetching schedule for doctor ID:', doctorId); // Debug log
        
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
            console.log('Schedules received:', schedules);
            if (Array.isArray(schedules)) {
                displayDoctorScheduleCalendar(schedules);
            } else {
                console.error('Schedules is not an array:', schedules);
            }
        })
        .catch(error => {
            console.error('Error fetching doctor schedule:', error);
        });
    }

    function displayDoctorScheduleCalendar(schedules) {
        // Create calendar container
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

        // Insert calendar before the date input
        const dateInput = document.getElementById('doctor-schedule-date');
        dateInput.parentNode.insertBefore(calendarContainer, dateInput);

        // Group schedules by date
        const schedulesByDate = {};
        schedules.forEach(schedule => {
            const date = schedule.workDate;
            if (!schedulesByDate[date]) {
                schedulesByDate[date] = [];
            }
            schedulesByDate[date].push(schedule);
        });

        // Generate calendar for current month
        generateCalendarMonth(schedulesByDate);
    }

    function generateCalendarMonth(schedulesByDate) {
        const calendarBody = document.getElementById('calendar-body');
        const today = new Date();
        const currentMonth = today.getMonth();
        const currentYear = today.getFullYear();

        // Get first day of month and number of days
        const firstDay = new Date(currentYear, currentMonth, 1);
        const lastDay = new Date(currentYear, currentMonth + 1, 0);
        const daysInMonth = lastDay.getDate();
        const startingDayOfWeek = firstDay.getDay();

        calendarBody.innerHTML = '';

        // Add empty cells for days before the first day of month
        for (let i = 0; i < startingDayOfWeek; i++) {
            const emptyCell = document.createElement('div');
            emptyCell.className = 'calendar-day empty';
            calendarBody.appendChild(emptyCell);
        }

        // Add days of the month
        for (let day = 1; day <= daysInMonth; day++) {
            const dayCell = document.createElement('div');
            dayCell.className = 'calendar-day';
            
            const date = new Date(currentYear, currentMonth, day);
            const dateStr = date.toISOString().split('T')[0];
            
            dayCell.innerHTML = `<div class="day-number">${day}</div>`;
            
            // Check if doctor has schedule for this date
            if (schedulesByDate[dateStr]) {
                dayCell.classList.add('has-schedule');
                const timesDiv = document.createElement('div');
                timesDiv.className = 'schedule-times';
                
                schedulesByDate[dateStr].forEach(schedule => {
                    const timeSlot = document.createElement('div');
                    timeSlot.className = `time-slot ${schedule.isAvailable ? 'available' : 'unavailable'}`;
                    timeSlot.textContent = `${schedule.startTime.substring(0,5)}-${schedule.endTime.substring(0,5)}`;
                    timeSlot.onclick = () => selectDoctorSchedule(schedule, dateStr);
                    timesDiv.appendChild(timeSlot);
                });
                
                dayCell.appendChild(timesDiv);
            }
            
            calendarBody.appendChild(dayCell);
        }
    }

    function selectDoctorSchedule(schedule, date) {
        // Update the date input
        document.getElementById('doctor-schedule-date').value = date;
        
        // Generate time slots for the selected schedule
        generateTimeSlots(schedule);
        
        // Show the time grid
        const timeGrid = document.getElementById('doctor-time-slot-grid');
        timeGrid.style.display = 'block';
    }

    function generateTimeSlots(schedule) {
        const timeGrid = document.getElementById('doctor-time-slot-grid');
        timeGrid.innerHTML = '';
        
        if (!schedule.isAvailable) {
            timeGrid.innerHTML = '<p>Doctor is not available on this date</p>';
            return;
        }
        
        // Generate 30-minute slots between start and end time
        const startTime = schedule.startTime.substring(0,5);
        const endTime = schedule.endTime.substring(0,5);
        const slots = generateTimeSlotsBetween(startTime, endTime);
        
        slots.forEach(slot => {
            const slotBtn = document.createElement('button');
            slotBtn.className = 'time-slot-btn';
            slotBtn.textContent = slot;
            slotBtn.onclick = () => {
                // Remove previous selection
                document.querySelectorAll('.time-slot-btn').forEach(btn => btn.classList.remove('selected'));
                slotBtn.classList.add('selected');
                selectedDoctorTimeSlot = slot;
                document.getElementById('confirm-doctor-booking-btn').style.display = 'block';
            };
            timeGrid.appendChild(slotBtn);
        });
    }

    function generateTimeSlotsBetween(startTime, endTime) {
        const slots = [];
        const start = new Date(`1970-01-01T${startTime}:00`);
        const end = new Date(`1970-01-01T${endTime}:00`);
        
        while (start < end) {
            slots.push(start.toTimeString().substring(0,5));
            start.setMinutes(start.getMinutes() + 30);
        }
        
        return slots;
    }

    // Initial fetch
    fetchDoctors();
    // Render initial time slots (if date is pre-filled)
    if (calendarDate.value) {
        renderTimeSlots(timeSlotGrid, (time, btn) => {
            if (selectedTimeBtn) selectedTimeBtn.classList.remove('selected');
            btn.classList.add('selected');
            selectedTimeBtn = btn;
            selectedTimeSlot = time;
            fetchAvailableDoctors(calendarDate.value, time);
        });
    }
    
    // Add a function to reset the booking form
    function resetBookingForm() {
        // Reset Order by Time section
        calendarDate.value = '';
        selectedTimeSlot = null;
        selectedDoctor = null;
        
        // Clear UI elements
        timeSlotGrid.innerHTML = '';
        doctorList.innerHTML = '';
        availableDoctorsSection.style.display = 'none';
        confirmTimeBookingBtn.style.display = 'none';
        
        // Remove selected states
        if (selectedTimeBtn) {
            selectedTimeBtn.classList.remove('selected');
            selectedTimeBtn = null;
        }
        
        // Reset Order by Doctor section
        doctorSelect.value = '';
        doctorScheduleDate.value = '';
        selectedDoctorId = null;
        selectedDoctorTimeSlot = null;
        
        // Hide doctor sections
        doctorScheduleSection.style.display = 'none';
        doctorTimeSlotGrid.innerHTML = '';
        confirmDoctorBookingBtn.style.display = 'none';
        
        // Remove existing calendar if any
        const existingCalendar = document.querySelector('.doctor-schedule-calendar');
        if (existingCalendar) {
            existingCalendar.remove();
        }
    }

    // Add some enhanced styling for the doctor cards
const additionalStyles = `
    .doctor-card {
        border: 2px solid #e0e0e0;
        border-radius: 8px;
        padding: 15px;
        margin: 10px 0;
        cursor: pointer;
        transition: all 0.3s ease;
        background: white;
    }
    
    .doctor-card:hover {
        border-color: #e74c3c;
        box-shadow: 0 4px 8px rgba(231, 76, 60, 0.1);
    }
    
    .doctor-card.selected {
        border-color: #e74c3c;
        background: #fff5f5;
    }
    
    .doctor-info h4 {
        margin: 0 0 8px 0;
        color: #e74c3c;
        font-size: 1.2em;
    }
    
    .doctor-degree {
        color: #666;
        font-weight: 500;
        margin: 5px 0;
    }
    
    .doctor-bio {
        color: #777;
        font-size: 0.9em;
        margin: 8px 0;
    }
    
    .doctor-details {
        margin-top: 10px;
        padding-top: 10px;
        border-top: 1px solid #eee;
    }
    
    .doctor-details small {
        display: block;
        color: #888;
        margin: 2px 0;
    }
    
    .no-doctors, .error-message {
        text-align: center;
        padding: 20px;
        color: #666;
        font-style: italic;
        background: #f8f9fa;
        border-radius: 8px;
        margin: 10px 0;
    }
    
    .error-message {
        color: #e74c3c;
        background: #fff5f5;
    }
    
    .no-doctors {
        color: #6c757d;
        background: #f8f9fa;
        border: 1px dashed #dee2e6;
    }
    
    .time-slot-btn {
        background: #f8f9fa;
        border: 1px solid #dee2e6;
        border-radius: 4px;
        padding: 8px 16px;
        margin: 5px;
        cursor: pointer;
        transition: all 0.2s ease;
    }
    
    .time-slot-btn:hover {
        background: #e9ecef;
        border-color: #adb5bd;
    }
    
    .time-slot-btn.selected {
        background: #e74c3c;
        color: white;
        border-color: #e74c3c;
    }
`;

// Add the styles to the page
const styleSheet = document.createElement('style');
styleSheet.textContent = additionalStyles;
document.head.appendChild(styleSheet);
});