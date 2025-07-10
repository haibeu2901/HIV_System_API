let doctors = [];
let selectedDoctor = null;
let selectedDate = null;
let selectedTime = null;
let selectedDayOfWeek = null;
const appointmentData = {
  date: null,
  time: null,
  doctor: null,
  type: null,
  notes: null,
};
let currentStep = 1;
const dayNames = [
  "Sunday",
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
];

// Only show Monday to Friday
const weekDays = [1, 2, 3, 4, 5]; // 1=Monday, ..., 5=Friday

// Fetch doctors from API
async function fetchDoctors() {
  const token = localStorage.getItem("token");
  const response = await fetch(
    "https://localhost:7009/api/Doctor/GetAllDoctors",
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  );
  if (!response.ok) throw new Error("Failed to fetch doctors");
  doctors = await response.json();
}

// Generate doctors grid using real data
async function generateDoctors() {
  await fetchDoctors();
  const doctorsGrid = document.getElementById("doctorsGrid");
  doctorsGrid.innerHTML = "";
  doctors.forEach((doctor) => {
    const doctorCard = document.createElement("div");
    doctorCard.className = "doctor-card";
    doctorCard.onclick = () => selectDoctor(doctor, doctorCard);
    doctorCard.innerHTML = `
            <div class="doctor-header">
                <div class="doctor-avatar"><i class="fas fa-user-md"></i></div>
                <div class="doctor-info">
                    <h3>${doctor.account.fullname}</h3>
                    <div class="doctor-specialty">${doctor.degree}</div>
                </div>
            </div>
            <div class="doctor-details">
                <div class="doctor-email">${doctor.account.email}</div>
                <div class="doctor-bio">${doctor.bio}</div>
            </div>
        `;
    doctorsGrid.appendChild(doctorCard);
  });
}

// When a doctor is selected, generate available dates for that doctor
function selectDoctor(doctor, cardElement) {
  document
    .querySelectorAll(".doctor-card")
    .forEach((card) => card.classList.remove("selected"));
  cardElement.classList.add("selected");
  selectedDoctor = doctor;
  appointmentData.doctor = doctor;
  // Reset date and time selections
  selectedDate = null;
  selectedTime = null;
  appointmentData.date = null;
  appointmentData.time = null;
  // Enable next button
  document.getElementById("nextBtn").disabled = false;
}

// When a date is selected, generate time slots for that date
function selectDate(date, cardElement) {
  document
    .querySelectorAll(".date-card")
    .forEach((card) => card.classList.remove("selected"));
  cardElement.classList.add("selected");
  selectedDate = date;
  appointmentData.date = date;
  document.getElementById("nextBtn").disabled = false;
  // Update selected date display
  const options = {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  };
  document.getElementById("selectedDateDisplay").textContent =
    date.toLocaleDateString("en-US", options);
}

// Generate time slots for the selected date and doctor
function generateTimeSlots() {
  const timeGrid = document.getElementById("timeGrid");
  timeGrid.innerHTML = "";
  if (!selectedDoctor || !selectedDate) return;
  const slots = generateTimeSlotsForDate(
    selectedDoctor.workSchedule,
    selectedDate
  );
  slots.forEach((time) => {
    const timeCard = document.createElement("div");
    timeCard.className = "time-slot";
    timeCard.textContent = time;
    timeCard.onclick = () => selectTime(time, timeCard);
    timeGrid.appendChild(timeCard);
  });
}

// When a time is selected
function selectTime(time, cardElement) {
  document
    .querySelectorAll(".time-slot")
    .forEach((card) => card.classList.remove("selected"));
  cardElement.classList.add("selected");
  selectedTime = time;
  appointmentData.time = time;
  document.getElementById("nextBtn").disabled = false;
}

// Generate time slots for a selected date
function generateTimeSlotsForDate(workSchedule, selectedDate) {
  const dayOfWeek = selectedDate.getDay();
  const slots = [];
  workSchedule
    .filter((ws) => ws.dayOfWeek === dayOfWeek)
    .forEach((ws) => {
      let start = ws.startTime.substring(0, 5);
      let end = ws.endTime.substring(0, 5);
      let [sh, sm] = start.split(":").map(Number);
      let [eh, em] = end.split(":").map(Number);
      let current = new Date(selectedDate);
      current.setHours(sh, sm, 0, 0);
      const endTime = new Date(selectedDate);
      endTime.setHours(eh, em, 0, 0);
      while (current < endTime) {
        let slotStart = current.toTimeString().substring(0, 5);
        current.setMinutes(current.getMinutes() + 30);
        let slotEnd =
          current < endTime ? current.toTimeString().substring(0, 5) : end;
        slots.push(`${slotStart} - ${slotEnd}`);
      }
    });
  return slots;
}

// Navigation functions
function nextStep() {
  if (currentStep < 4) {
    currentStep++;
    updateStepVisibility();
    if (currentStep === 2) {
      renderWeekDays(selectedDoctor.workSchedule); // <-- Use this!
    } else if (currentStep === 3) {
      renderTimeSlotsForDay(selectedDoctor.workSchedule, selectedDayOfWeek);
    } else if (currentStep === 4) {
      updateConfirmation();
    }
    document.getElementById("nextBtn").disabled = true;
  }
}

function previousStep() {
  if (currentStep > 1) {
    currentStep--;
    updateStepVisibility();
    document.getElementById("nextBtn").disabled = false;
  }
}

// Update step visibility and navigation
function updateStepVisibility() {
  document.querySelectorAll(".step").forEach((step, index) => {
    step.classList.remove("active", "completed");
    if (index + 1 === currentStep) {
      step.classList.add("active");
    } else if (index + 1 < currentStep) {
      step.classList.add("completed");
    }
  });
  document.querySelectorAll(".step-content").forEach((content, index) => {
    content.classList.remove("active");
    if (index + 1 === currentStep) {
      content.classList.add("active");
    }
  });
  const prevBtn = document.getElementById("prevBtn");
  const nextBtn = document.getElementById("nextBtn");
  const confirmBtn = document.getElementById("confirmBtn");
  prevBtn.style.display = currentStep > 1 ? "inline-flex" : "none";
  if (currentStep === 4) {
    nextBtn.classList.add("hidden");
    confirmBtn.classList.remove("hidden");
  } else {
    nextBtn.classList.remove("hidden");
    confirmBtn.classList.add("hidden");
    nextBtn.disabled = !isStepValid();
  }
}

// Check if current step is valid
function isStepValid() {
  switch (currentStep) {
    case 1:
      return selectedDoctor !== null;
    case 2:
      return selectedDayOfWeek !== null;
    case 3:
      return selectedTime !== null;
    case 4:
      return true; // Only notes are optional
    default:
      return false;
  }
}

// Update confirmation details
function updateConfirmation() {
  const options = {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  };
  document.getElementById("confirmDate").textContent = selectedDate
    ? selectedDate.toLocaleDateString("en-US", options)
    : "";
  document.getElementById("confirmTime").textContent = selectedTime || "";
  document.getElementById("confirmDoctor").textContent = selectedDoctor
    ? selectedDoctor.account.fullname
    : "";
}

// Confirm appointment (unchanged)
function confirmAppointment() {
  if (!selectedDoctor || selectedDayOfWeek === null || !selectedTime) {
    alert("Please complete all steps.");
    return;
  }
  const appointmentDate = getNextDateForDay(selectedDayOfWeek);
  const appointmentTime = getStartTimeFromSlot(selectedTime);
  const notes = document.getElementById("notes").value || "";

  const payload = {
    doctorId: selectedDoctor.doctorId,
    appointmentDate: getNextDateForDay(selectedDayOfWeek),
    appointmentTime: getStartTimeFromSlot(selectedTime),
    notes: document.getElementById("notes")?.value || "",
  };
  console.log(payload);
  const token = localStorage.getItem("token");
  fetch("https://localhost:7009/api/Appointment/CreateAppointment", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },

    body: JSON.stringify(payload),
  })
    .then((res) => {
      if (res.status === 409) {
        // Show the backend's message if available
        return res.text().then((msg) => {
          alert(
            msg ||
              "This time slot has already been taken. Please choose another time slot."
          );
          throw new Error("Time slot taken");
        });
      }
      if (!res.ok) throw new Error("Failed to create appointment");
      return res.json();
    })
    .then((data) => {
      window.location.href = "../appointment-view/view-appointment.html";
    })
    .then((res) => {
      if (res.status === 409) {
        // Show the backend's message if available
        return res.text().then((msg) => {
          alert(
            msg ||
              "This time slot has already been taken. Please choose another time slot."
          );
          throw new Error("Time slot taken");
        });
      }
      if (!res.ok) throw new Error("Failed to create appointment");
      return res.json();
    })
    .then((data) => {
      window.location.href = "../appointment-view/view-appointment.html";
    })
    .catch((err) => {
      if (err.message !== "Time slot taken") {
        alert("Error: " + err.message);
      }
    });
}

// Load header
fetch("../header/header.html")
  .then((response) => response.text())
  .then((data) => {
    document.getElementById("header-placeholder").innerHTML = data;
  });

// On page load
document.addEventListener("DOMContentLoaded", async function () {
  const urlParams = new URLSearchParams(window.location.search);
  const doctorId = urlParams.get("doctorId");
  if (doctorId) {
    // Fetch doctor by ID and pre-select
    const token = localStorage.getItem("token");
    const response = await fetch(
      `https://localhost:7009/api/Doctor/GetDoctorById?id=${doctorId}`,
      {
        headers: { Authorization: `Bearer ${token}` },
      }
    );
    if (response.ok) {
      selectedDoctor = await response.json();
      appointmentData.doctor = selectedDoctor;
      // Render only this doctor as selected
      const doctorsGrid = document.getElementById("doctorsGrid");
      doctorsGrid.innerHTML = `
                <div class="doctor-card selected">
                    <div class="doctor-header">
                        <div class="doctor-avatar"><i class="fas fa-user-md"></i></div>
                        <div class="doctor-info">
                            <h3>${selectedDoctor.account.fullname}</h3>
                            <div class="doctor-specialty">${selectedDoctor.degree}</div>
                        </div>
                    </div>
                    <div class="doctor-details">
                        <div class="doctor-email">${selectedDoctor.account.email}</div>
                        <div class="doctor-bio">${selectedDoctor.bio}</div>
                    </div>
                </div>
            `;
      document.getElementById("nextBtn").disabled = false;
    }
  } else {
    // No doctorId, show all doctors for selection
    generateDoctors();
  }
  updateStepVisibility();
  if (selectedDoctor) {
    renderWeekDays(selectedDoctor.workSchedule);
  }
});

function getDoctorAvailableDays(workSchedule) {
  // Returns array of day names, e.g. ["Monday", "Wednesday", "Friday"]
  const uniqueDays = [...new Set(workSchedule.map((ws) => ws.dayOfWeek))];
  return uniqueDays.map((dayNum) => dayNames[dayNum]);
}

function getTimeSlotsForDay(workSchedule, dayNum) {
  // Returns array of time slot strings for the selected day
  const slots = [];
  workSchedule
    .filter((ws) => ws.dayOfWeek === dayNum)
    .forEach((ws) => {
      let [sh, sm] = ws.startTime.split(":").map(Number);
      let [eh, em] = ws.endTime.split(":").map(Number);
      let current = new Date();
      current.setHours(sh, sm, 0, 0);
      const end = new Date();
      end.setHours(eh, em, 0, 0);
      while (current < end) {
        let slotStart = current.toTimeString().substring(0, 5);
        current.setMinutes(current.getMinutes() + 30);
        let slotEnd =
          current < end
            ? current.toTimeString().substring(0, 5)
            : ws.endTime.substring(0, 5);
        slots.push(`${slotStart} - ${slotEnd}`);
      }
    });
  return slots;
}

function renderWeekDays(workSchedule) {
  const dateGrid = document.getElementById("dateGrid");
  dateGrid.innerHTML = "";
  // Find unique days from the doctor's schedule
  const availableDays = [...new Set(workSchedule.map((ws) => ws.dayOfWeek))];
  for (let dayNum = 0; dayNum < 7; dayNum++) {
    const hasSlot = availableDays.includes(dayNum);
    const btn = document.createElement("button");
    btn.className = "weekday-btn";
    btn.textContent = dayNames[dayNum];
    btn.disabled = !hasSlot;
    if (hasSlot) {
      btn.onclick = () => selectWeekDay(dayNum, btn);
    }
    dateGrid.appendChild(btn);
  }
}

function selectWeekDay(dayNum, btn) {
  // Highlight selected button
  document
    .querySelectorAll(".weekday-btn")
    .forEach((b) => b.classList.remove("selected"));
  btn.classList.add("selected");
  // Show time slots for this day
  renderTimeSlotsForDay(selectedDoctor.workSchedule, dayNum);
  selectedDayOfWeek = dayNum;
  document.getElementById("nextBtn").disabled = false;
}

// When rendering time slots:
function renderTimeSlotsForDay(workSchedule, dayNum) {
  const timeGrid = document.getElementById("timeGrid");
  timeGrid.innerHTML = "";
  const slots = [];
  workSchedule
    .filter((ws) => ws.dayOfWeek === dayNum)
    .forEach((ws) => {
      let [sh, sm] = ws.startTime.split(":").map(Number);
      let [eh, em] = ws.endTime.split(":").map(Number);
      let current = new Date(2000, 0, 1, sh, sm, 0, 0);
      const end = new Date(2000, 0, 1, eh, em, 0, 0);
      while (current < end) {
        let slotStart = current.toTimeString().substring(0, 5);
        current.setMinutes(current.getMinutes() + 30);
        let slotEnd =
          current <= end
            ? current.toTimeString().substring(0, 5)
            : ws.endTime.substring(0, 5);
        slots.push(`${slotStart} - ${slotEnd}`);
      }
    });
  // Render slots
  slots.forEach((slot) => {
    const slotBtn = document.createElement("button");
    slotBtn.className = "time-slot-btn";
    slotBtn.textContent = slot;
    slotBtn.onclick = () => selectTimeSlot(slot, slotBtn);
    timeGrid.appendChild(slotBtn);
  });
  // Disable Next button until a slot is selected
  document.getElementById("nextBtn").disabled = true;
}

function selectTimeSlot(slot, btn) {
  document
    .querySelectorAll(".time-slot-btn")
    .forEach((b) => b.classList.remove("selected"));
  btn.classList.add("selected");
  selectedTime = slot;
  // Enable Next button
  document.getElementById("nextBtn").disabled = false;
}

function getNextDateForDay(dayOfWeek) {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const todayDay = today.getDay();
  let diff = dayOfWeek - todayDay;
  if (diff < 0) diff += 7;
  const targetDate = new Date(today);
  targetDate.setDate(today.getDate() + diff);
  // Format as YYYY-MM-DD in local time
  const yyyy = targetDate.getFullYear();
  const mm = String(targetDate.getMonth() + 1).padStart(2, "0");
  const dd = String(targetDate.getDate()).padStart(2, "0");
  return `${yyyy}-${mm}-${dd}`;
}

function getStartTimeFromSlot(slot) {
  // slot is "14:00 - 14:30"
  return slot.split(" - ")[0] + ":00";
}
