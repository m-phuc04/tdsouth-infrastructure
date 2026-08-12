<?php

$serverName = "localhost\\SQLEXPRESS";

$connectionOptions = array(
    "Database" => "TDSouth",
    "Uid" => "sa",
    "PWD" => "admin@123",
    "TrustServerCertificate" => true
);

$conn = sqlsrv_connect($serverName, $connectionOptions);

if ($conn) {
    echo "<h2 style='color:green'>Kết nối SQL Server thành công!</h2>";
} else {
    echo "<pre>";
    print_r(sqlsrv_errors());
    echo "</pre>";
}

?>