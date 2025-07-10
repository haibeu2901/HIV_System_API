document.addEventListener("DOMContentLoaded", async () => {
  const token = localStorage.getItem("token");
  const calendarEl = document.getElementById("calendar");

  let events = [];


  try {
    const res = await fetch(
      "https://localhost:7009/api/Appointment/GetAllPersonalAppointments",
      {
        headers: { Authorization: `Bearer ${token}` },
      }
    );
    if (!res.ok) throw new Error("Failed to fetch appointments");
    const data = await res.json();

    events = data.map((appt) => ({
      title: `Dr. ${appt.doctorName}\n${appt.apmTime} (${renderStatusText(
        appt.apmStatus
      )})`,
      start: `${appt.apmtDate}T${appt.apmTime}`,
      end: `${appt.apmtDate}T${addMinutes(appt.apmTime, 30)}`, // 30 min duration
      color: getStatusColor(appt.apmStatus),
      extendedProps: {
        notes: appt.notes || "None",
        status: renderStatusText(appt.apmStatus),
      },
    }));
  } catch (err) {
    calendarEl.innerHTML = `<p style="color:red;">${err.message}</p>`;
    return;
  }

  const calendar = new FullCalendar.Calendar(calendarEl, {
    initialView: "dayGridMonth",
    headerToolbar: {
      left: "prev,next today",
      center: "title",
      right: "dayGridMonth,timeGridWeek,timeGridDay",
    },
    events: events,
    eventDisplay: "block",
    eventTimeFormat: {
      hour: "2-digit",
      minute: "2-digit",
      meridiem: false,
      hour12: false,
    },
    displayEventTime: false,
    eventDidMount: function (info) {
      // Make events look like sticky notes
      info.el.style.borderRadius = "12px";
      info.el.style.boxShadow = "0 2px 8px rgba(44,62,80,0.10)";
      info.el.style.fontWeight = "500";
      info.el.style.whiteSpace = "pre-line";
    },
    eventClick: function (info) {
      // Fill modal fields
      document.getElementById("sticky-title").textContent =
        info.event.title.split("\n")[0];
      document.getElementById("sticky-status").textContent =
        info.event.extendedProps.status;
      document.getElementById("sticky-date").textContent =
        info.event.start.toLocaleDateString();
      document.getElementById("sticky-time").textContent =
        info.event.start.toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        }) +
        " - " +
        info.event.end.toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        });
      document.getElementById("sticky-notes").textContent =
        info.event.extendedProps.notes;

      // Show modal
      document.getElementById("sticky-note-modal").classList.remove("hidden");
    },
  });
  calendar.render();

  document.getElementById("closeStickyNote").onclick = function () {
    document.getElementById("sticky-note-modal").classList.add("hidden");
  };
  document.getElementById("sticky-note-modal").onclick = function (e) {
    if (e.target === this) this.classList.add("hidden");
  };
});

function renderStatusText(status) {
  switch (status) {
    case 1:
      return "Pending";
    case 2:
      return "Confirmed";
    case 3:
      return "Cancelled";
    default:
      return "Unknown";
  }
}
function getStatusColor(status) {
  switch (status) {
    case 1:
      return "#fbc02d"; // Pending - yellow
    case 2:
      return "#43a047"; // Confirmed - green
    case 3:
      return "#e53935"; // Cancelled - red
    default:
      return "#90a4ae";
  }
}

function renderStatusText(status) {
  switch (status) {
    case 1:
      return "Pending";
    case 2:
      return "Confirmed";
    case 3:
      return "Cancelled";
    default:
      return "Unknown";
  }
}
function getStatusColor(status) {
  switch (status) {
    case 1:
      return "#fbc02d"; // Pending - yellow
    case 2:
      return "#43a047"; // Confirmed - green
    case 3:
      return "#e53935"; // Cancelled - red
    default:
      return "#90a4ae";
  }
}
function addMinutes(time, minsToAdd) {
  // time: "09:00", minsToAdd: 30 => "09:30"
  const [h, m] = time.split(":").map(Number);
  const date = new Date(0, 0, 0, h, m + minsToAdd, 0, 0);
  return date.toTimeString().slice(0, 5);
}
