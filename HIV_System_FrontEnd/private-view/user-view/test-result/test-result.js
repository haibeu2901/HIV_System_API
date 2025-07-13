document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem("token");
    const container = document.getElementById('result-container');

    // Show loading state
    container.innerHTML = `
        <div class="loading-container">
            <div class="loader"></div>
            <p>Loading test results...</p>
        </div>
    `;

    fetch("https://localhost:7009/api/TestResult/personal", {
        headers: { 
            "Authorization": `Bearer ${token}`,
            "accept": "*/*"
        }
    })
    .then(res => {
        if (!res.ok) {
            throw new Error(`HTTP error! status: ${res.status}`);
        }
        return res.json();
    })
    .then(data => {
        console.log('API Response:', data);
        
        if (!data || data.length === 0) {
            container.innerHTML = `
                <div class="no-results">
                    <div class="no-results-icon">
                        <i class="fa-solid fa-flask-vial"></i>
                    </div>
                    <h3>No Test Results Found</h3>
                    <p>You don't have any test results yet. Please contact your healthcare provider for more information.</p>
                </div>
            `;
            return;
        }

        // Display raw data from API response
        let resultsHTML = '<div class="test-results-raw">';
        
        data.forEach((testResult, index) => {
            resultsHTML += `
                <div class="test-result-item">
                    <h3>Test Result ${index + 1}</h3>
                    <div class="raw-data">
                        <div class="data-field">
                            <span class="field-label">Patient Medical Record ID:</span>
                            <span class="field-value">${testResult.patientMedicalRecordId}</span>
                        </div>
                        <div class="data-field">
                            <span class="field-label">Result:</span>
                            <span class="field-value">${testResult.result}</span>
                        </div>
                        <div class="data-field">
                            <span class="field-label">Test Date:</span>
                            <span class="field-value">${testResult.testDate}</span>
                        </div>
                        <div class="data-field">
                            <span class="field-label">Notes:</span>
                            <span class="field-value">${testResult.notes}</span>
                        </div>
                    </div>
                </div>
            `;
        });
        
        resultsHTML += '</div>';
        
        container.innerHTML = resultsHTML;
    })
    .catch(error => {
        console.error('Error loading test results:', error);
        container.innerHTML = `
            <div class="error-container">
                <div class="error-icon">
                    <i class="fa-solid fa-exclamation-triangle"></i>
                </div>
                <h3>Error Loading Test Results</h3>
                <p>We couldn't load your test results at this time. Please try again later or contact support if the problem persists.</p>
                <button onclick="location.reload()" class="retry-btn">
                    <i class="fa-solid fa-refresh"></i> Try Again
                </button>
            </div>
        `;
    });
});