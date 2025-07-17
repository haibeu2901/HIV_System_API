document.addEventListener("DOMContentLoaded", async () => {
    const token = localStorage.getItem("token");
    if (!token) {
        window.location.href = "../../public-view/landingpage.html";
        return;
    }

    try {
        const response = await fetch("https://localhost:7009/api/Account/View-profile", {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) throw new Error("Failed to fetch profile");

        const data = await response.json();

        // Render editable form
        document.getElementById("profile-details").innerHTML = `
            <form id="profileForm">
                <ul class="profile-list">
                    <li><strong>ID tài khoản:</strong> <span>${data.accId}</span></li>
                    <li><strong>Tên đăng nhập:</strong> <span>${data.accUsername}</span></li>
                    <li>
                        <strong>Email:</strong>
                        <input type="email" id="email" name="email" value="${data.email}" readonly style="background-color: #f5f5f5; cursor: not-allowed;" />
                    </li>
                    <li>
                        <strong>Họ và tên:</strong>
                        <input type="text" id="fullname" name="fullname" value="${data.fullname}" required />
                    </li>
                    <li>
                        <strong>Ngày sinh:</strong>
                        <input type="date" id="dob" name="dob" value="${data.dob ? data.dob.substring(0,10) : ''}" required />
                    </li>
                    <li>
                        <strong>Giới tính:</strong>
                        <select id="gender" name="gender">
                            <option value="true" ${data.gender ? "selected" : ""}>Nam</option>
                            <option value="false" ${!data.gender ? "selected" : ""}>Nữ</option>
                        </select>
                    </li>
                    <li>
                        <strong>Mật khẩu:</strong>
                        <span>********</span>
                    </li>
                    <li>
                        <strong>Vai trò:</strong> <span>${data.roles}</span>
                    </li>
                    <li>
                        <strong>Trạng thái:</strong> <span>${data.isActive ? "Đang hoạt động" : "Không hoạt động"}</span>
                    </li>
                </ul>
                <button type="submit" class="btn-profile" style="margin-top:1rem;">Lưu thay đổi</button>
            </form>
        `;

        // Handle form submission
        document.getElementById("profileForm").addEventListener("submit", async function(e) {
            e.preventDefault();

            const fullname = document.getElementById("fullname").value.trim();
            const dob = document.getElementById("dob").value;
            const gender = document.getElementById("gender").value === "true";

            // Validate required fields
            if (!fullname.trim()) {
                showMessage("Vui lòng nhập họ và tên hợp lệ.", true);
                return;
            }

            if (!dob) {
                showMessage("Vui lòng chọn ngày sinh.", true);
                return;
            }

            const updateData = {
                fullname,
                dob,
                gender
            };

           

            try {
                console.log('Sending update request with data:', updateData);
                
                const updateResponse = await fetch("https://localhost:7009/api/Account/UpdatePatientProfile", {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    },
                    body: JSON.stringify(updateData)
                });

                console.log('Update response status:', updateResponse.status);
                console.log('Update response ok:', updateResponse.ok);

                // Get response text
                const responseText = await updateResponse.text();
                console.log('Response text:', responseText);

                // Check if the request was successful (status 200-299)
                if (updateResponse.ok) {
                    // Success case - status 200-299
                    if (responseText && responseText.includes('error')) {
                        // API returned success status but with error message
                        showMessage("Cập nhật hoàn tất với cảnh báo: " + responseText, false);
                    } else {
                        // Complete success
                        showMessage("Cập nhật hồ sơ thành công!", false);
                        
                        // Optionally refresh the page after a short delay
                        setTimeout(() => {
                            window.location.reload();
                        }, 2000);
                    }
                } else {
                    // Error case - status 400-599
                    console.log('Error response:', responseText);
                    throw new Error(responseText || `HTTP Error ${updateResponse.status}`);
                }
                
            } catch (err) {
                console.error('Update error:', err);
                showMessage("Cập nhật thất bại: " + err.message, true);
            }
        });

    } catch (err) {
        document.getElementById("profile-details").innerHTML = `<p style="color:red;">Lỗi khi tải thông tin hồ sơ.</p>`;
    }
});

// Helper to show messages
function showMessage(msg, isError) {
    const msgDiv = document.getElementById("profile-message");
    msgDiv.textContent = msg;
    msgDiv.style.color = isError ? "red" : "green";
    setTimeout(() => { msgDiv.textContent = ""; }, 4000);
}