
function Delete(id) {
    $ajax({
        type: 'post',
        success: function (response) {
            if (response == null || response == undefined) {
                alert('無法刪除資料');
            }
            else if (response.length == 0) {
                alert('找不到資料'+id);
            }
        }

    })
}