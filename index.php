<?php

$serverName = "localhost\\SQLEXPRESS";

$connectionOptions = array(
    "Database" => "TDSouth",
    "Uid" => "sa",
    "PWD" => "admin@123",
    "TrustServerCertificate" => true
);

$conn = sqlsrv_connect($serverName, $connectionOptions);

if (!$conn) {
    die(print_r(sqlsrv_errors(), true));
}

// Thêm nhân viên
if (isset($_POST["btnSave"])) {

    $manv = $_POST["manv"];
    $hoten = $_POST["hoten"];
    $phongban = $_POST["phongban"];
    $luong = $_POST["luong"];

    $sql = "INSERT INTO NhanVien(MaNV, HoTen, PhongBan, Luong)
            VALUES(?,?,?,?)";

    $params = array(
        $manv,
        $hoten,
        $phongban,
        $luong
    );

    $stmt = sqlsrv_query($conn, $sql, $params);

    if ($stmt) {
        echo "<script>alert('Thêm nhân viên thành công!');</script>";
    } else {
        echo "<script>alert('Lỗi khi thêm dữ liệu!');</script>";
        echo "<pre>";
        print_r(sqlsrv_errors());
        echo "</pre>";
    }
}

?>

<!DOCTYPE html>
<html lang="vi">
<head>
<meta charset="UTF-8">
<title>Quản Lý Nhân Viên</title>

<style>

body{
    font-family:Arial;
    margin:40px;
    background:#f5f5f5;
}

.container{
    width:900px;
    margin:auto;
    background:white;
    padding:20px;
    border-radius:10px;
}

input{
    width:300px;
    padding:8px;
}

button{
    padding:10px 25px;
    background:#2196F3;
    color:white;
    border:none;
    cursor:pointer;
}

table{
    width:100%;
    border-collapse:collapse;
    margin-top:20px;
}

table,th,td{
    border:1px solid #ccc;
}

th{
    background:#2196F3;
    color:white;
}

th,td{
    padding:10px;
    text-align:center;
}

</style>

</head>

<body>

<div class="container">

<h2 align="center">HỆ THỐNG QUẢN LÝ NHÂN VIÊN</h2>

<form method="post">

<b>Mã nhân viên</b><br>
<input type="number" name="manv" required>

<br><br>

<b>Họ tên</b><br>
<input type="text" name="hoten" required>

<br><br>

<b>Phòng ban</b><br>
<input type="text" name="phongban" required>

<br><br>

<b>Lương</b><br>
<input type="number" name="luong" required>

<br><br>

<button type="submit" name="btnSave">
Lưu nhân viên
</button>

</form>

<hr>

<h3>Danh sách nhân viên</h3>

<table>

<tr>

<th>Mã NV</th>

<th>Họ tên</th>

<th>Phòng ban</th>

<th>Lương</th>

</tr>

<?php

$sql="SELECT * FROM NhanVien ORDER BY MaNV";

$result=sqlsrv_query($conn,$sql);

while($row=sqlsrv_fetch_array($result,SQLSRV_FETCH_ASSOC))
{

?>

<tr>

<td><?php echo $row["MaNV"]; ?></td>

<td><?php echo $row["HoTen"]; ?></td>

<td><?php echo $row["PhongBan"]; ?></td>

<td><?php echo number_format($row["Luong"]); ?></td>

</tr>

<?php

}

?>

</table>

</div>

</body>
</html>