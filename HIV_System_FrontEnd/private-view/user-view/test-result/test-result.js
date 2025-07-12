document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem("token");
    const container = document.getElementById('result-container');

    fetch("https://localhost:7009/api/TestResult/personal", {
        headers: { "Authorization": `Bearer ${token}` }
    })
    .then(res => res.json())
    .then(data => {
        if (!data) {
            container.innerHTML = "<p>No test result found.</p>";
            return;
        }
        const isPositive = data.result === true;
        container.innerHTML = `
            <div class="result-icon ${isPositive ? '' : 'negative'}">
                <i class="fa-solid ${isPositive ? 'fa-check-circle' : 'fa-xmark-circle'}"></i>
            </div>
            <div class="result-title">
                ${isPositive ? 'Test Result: Good' : 'Test Result: Attention Needed'}
            </div>
            <div class="result-date">
                <i class="fa-regular fa-calendar"></i> ${data.testDate}
            </div>
            <div class="result-notes">
                ${data.notes ? data.notes : ''}
            </div>
        `;
    })
    .catch(() => {
        container.innerHTML = "<p style='color:red;'>Error loading test result.</p>";
    });
});