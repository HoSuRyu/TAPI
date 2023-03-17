//마우스 오른쪽 메뉴 막기
$(document).contextmenu(function (event) {
    event.preventDefault();
    return false;
});

//라디오 클릭
$(":radio").click(function () {
    CheckOpenBox();
    $('tr[stepbystep="' + $(this).attr('NextStep') + '"][stepVariable="' + $(this).attr('stepVariable') + '"]').show();

    //maxdiff 같은거 선택 못하게
    var variable = $(this).attr('Variable');
    var noSelectVariable = '';
    var code = $(this).attr('value');

    if (variable.substr(variable.length - 1) == 'b') {
        noSelectVariable = variable.replace("b", "w");
    }
    else if (variable.substr(variable.length - 1) == 'w') {
        noSelectVariable = variable.replace("w", "b");
    }

    $(':radio[Variable="' + noSelectVariable + '"]').removeAttr('disabled');
    $(':radio[Variable="' + noSelectVariable + '"][value="' + code + '"]').prop('checked', false);
    $(':radio[Variable="' + noSelectVariable + '"][value="' + code + '"]').attr('disabled', '');

    var nextVariable = $(this).attr('nextfocus');

    if ($(this).is(':radio[nextfocus!="lastfocus"]') == true) {
        $(':text[Variable="' + nextVariable + '"],select[Variable="' + nextVariable + '"],:radio[Variable="' + nextVariable + '"],:checkbox[Variable="' + nextVariable + '"]').focus();
    }
});

//멀티 랭킹 체크
$(":hidden[id^='RankingValue_']").each(function () {
    var rankingVariable = $(this).attr('id').replace('RankingValue_', '');
    CheckRanking(rankingVariable, $(this).val(), $(this).attr('isrank'));
});

//체크박스 클릭
$(":checkbox").click(function () {
    checkboxClickType = 0;
    var variableType = 'rankingVariable';
    var rankingVariable = $(this).attr('rankingVariable');

    if (typeof rankingVariable == 'undefined') {
        variableType = 'openVariable';
        rankingVariable = $(this).attr('openVariable');
    }

    var rankingValue = ',' + $("#RankingValue_" + rankingVariable).val() + ',';
    var checkCode = $(this).attr('value');

    if ($(this).prop('checked')) {
        if (rankingValue == '') {
            rankingValue = ',' + $(this).attr('value') + ',';
        }
        else {
            rankingValue = rankingValue + ',' + $(this).attr('value') + ',';
        }

        if ($(this).attr('exclusive') == 'True') {
            $(':checkbox[exclusive="False"][' + variableType + '="' + rankingVariable + '"]').prop('checked', false);
            rankingValue = ',' + $(this).attr('value') + ',';

            $(':checkbox[exclusive="True"][' + variableType + '="' + rankingVariable + '"]').each(function () {
                if (checkCode != $(this).attr('value')) {
                    $(this).prop('checked', false);
                    rankingValue = rankingValue.replace(',' + $(this).attr('value') + ',', ',');
                }
            });
        }
        else {
            $(':checkbox[exclusive="True"][' + variableType + '="' + rankingVariable + '"]').each(function () {
                $(this).prop('checked', false);
                rankingValue = rankingValue.replace(',' + $(this).attr('value') + ',', ',');
            });
        }
    }
    else {
        rankingValue = rankingValue.replace(',' + $(this).attr('value') + ',', ',');
    }

    //앞뒤 트림
    rankingValue = rankingValue.replace(/,,/g, ',');
    rankingValue = rankingValue.replace(/,$/g, '');
    rankingValue = rankingValue.replace(/^,/g, '');
    rankingValue = rankingValue.replace(/^,/g, '');

    $("#RankingValue_" + rankingVariable).val(rankingValue);

    CheckOpenBox();
    $('tr[stepbystep="' + $(this).attr('NextStep') + '"][stepVariable="' + $(this).attr('stepVariable') + '"]').show();
    CheckRanking(rankingVariable, rankingValue, $(this).attr('isrank'));
});

$("select[nextfocus]").change(function () {
    var $focused = $(':focus');

    if ($focused.is('select[nextfocus!="lastfocus"]') == true) {
        $(':text[Variable="' + $focused.attr('nextfocus') + '"],select[Variable="' + $focused.attr('nextfocus') + '"],div[Variable="' + $focused.attr('nextfocus') + '"]').focus();
    }
});

//그리드 오픈 입력 된 것만 선택 가능
$('textarea[id!="debug"],:text[id!="debug"]').keyup(function () {
    var openVariable = $(this).attr('variable');
    if ($(this).val() == '') {
        $(':radio[openVariable="' + openVariable + '"],:checkbox[openVariable="' + openVariable + '"]').attr('disabled', '');
        $(':radio[openVariable="' + openVariable + '"],:checkbox[openVariable="' + openVariable + '"]').prop('checked', false);
    }
    else {
        $(':radio[openVariable="' + openVariable + '"],:checkbox[openVariable="' + openVariable + '"]').removeAttr('disabled');
    }
});

//텍스트 박스 형식 지정
$('textarea[openDataType^="numeric"],:text[openDataType^="numeric"]').keydown(function (event) {
    var charCode = (event.which) ? event.which : event.keyCode;

    if (isNumeric(charCode, this) == false) {
        event.preventDefault();
    }
});

$('textarea[openDataType^="decimals"],:text[openDataType^="decimals"]').keydown(function (event) {
    var charCode = (event.which) ? event.which : event.keyCode;

    if (isDecimal(charCode, this) == false) {
        event.preventDefault();
    }
});

//숫자를 한글로와 콤마 찍기 - 값이 있는경우
$('textarea[ShowHG!="-1"],input:text[ShowHG!="-1"]').each(function () {
    $("#numberToKorean_" + $(this).attr('variable')).text(numberToKorean($(this).val(), $(this).attr('ShowHG')));
});

$('textarea[openDataType$="comma"],input:text[openDataType$="comma"]').each(function () {
    var $this = $(this);
    var nums = $this.val().split('.');

    num = nums[0].replace(/,/g, '')
    var numPoint = '';

    if (nums.length > 1) {
        numPoint = "." + nums[1];
    }

    $this.val(num.replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,") + numPoint);
});

//숫자를 한글로와 콤마 찍기 - 값을 입력 하는경우
$('textarea[ShowHG!="-1"],input:text[ShowHG!="-1"]').keyup(function () {
    $("#numberToKorean_" + $(this).attr('variable')).text(numberToKorean($(this).val(), $(this).attr('ShowHG')));
});

$('textarea[openDataType$="comma"],input:text[openDataType$="comma"]').keyup(function (event) {
    if (event.which == 36 || event.which == 37 || event.which == 38 || event.which == 39 || event.which == 40) {
        return false;
    }

    var $this = $(this);
    var nums = $this.val().split('.');

    num = nums[0].replace(/,/g, '')
    var numPoint = '';

    if (nums.length > 1) {
        numPoint = "." + nums[1];
    }

    $this.val(num.replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,") + numPoint);
});

//textarea maxlen 안 먹히네 스크립트로
//    $('textarea[maxLength!=""]').keyup(function () {
//        var maxlength = $(this).attr("maxLength");
//        var lineBreaks = $(this).val().split('\n').length - 1;
//        var charsUsed = $(this).val().length + lineBreaks;
//        if (charsUsed >= maxlength) {
//            $(this).val($(this).val().substr(0, maxlength - lineBreaks));
//            charsUsed = maxlength;
//        }
//    });

//AutoSum
$('textarea[openDataType^="numeric"],textarea[openDataType^="decimals"],input:text[openDataType^="numeric"],input:text[openDataType^="decimals"]').keyup(function () {
    if ($('textarea[AutoSum^="True"],input:text[AutoSum^="True"]').length > 0) {
        var autoSum = 0;

        $('textarea[openDataType^="numeric"],textarea[openDataType^="decimals"],input:text[openDataType^="numeric"],input:text[openDataType^="decimals"]').each(function () {
            autoSum = autoSum + Number(this.value.replace(/,/g, ''));
        });

        $('textarea[AutoSum^="True"],input:text[AutoSum^="True"]').each(function () {
            $(this).val(autoSum);

            if ($(this).attr('ShowHG') != '-1') {
                $("#numberToKorean_" + $(this).attr('variable')).text(numberToKorean($(this).val(), $(this).attr('ShowHG')));
            }

            if ($(this).attr('openDataType').indexOf('comma') > -1) {
                var $this = $(this);
                var nums = $this.val().split('.');

                num = nums[0].replace(/,/g, '')
                var numPoint = '';

                if (nums.length > 1) {
                    numPoint = "." + nums[1];
                }

                $this.val(num.replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,") + numPoint);
            }
        });
    }
});

var checkboxClickType = 1;

$(document).ready(function () {
    $("#debug").replaceWith($("#DebugData").val());

    //오픈 선택 된 것만 보이기
    CheckOpenBox();

    //그리드 스텝바이스텝
    CheckStepByStep();

    checkboxClickType = 1;
});

//숫자만 입력
function isNumeric(charCode, textBox)
{
    if ((charCode > 47 && charCode < 58) || //1~9 키보드 위 숫자
        (charCode > 95 && charCode < 106) || //1~9 키패드 숫자
        ((charCode == 109 || charCode == 189) && textBox.value.length == 0) || //-(키보드 189, 키패드 109)는 처음에만 가능
        charCode == 8 || //backspace
        charCode == 9 || //tab
        charCode == 46 || //del
        charCode == 36 || //home
        charCode == 35 || //end
        charCode == 37 || //<- 화살표
        charCode == 39) //-> 화살표
    {             
        return true; 
    }

    return false;
}

//실수만 입력
function isDecimal(charCode, textBox)
{
    if ((charCode > 47 && charCode < 58) || //1~9 키보드 위 숫자
        (charCode > 95 && charCode < 106) || //1~9 키패드 숫자
        ((charCode == 109 || charCode == 189) && textBox.value.length == 0) || //-(키보드 189, 키패드 109)는 처음에만 가능
        ((charCode == 110 || charCode == 190) && textBox.value.length > 0 && textBox.value.indexOf('.') == -1) || //소수점은 한번만, 처음에는 불가능
        charCode == 8 || //backspace
        charCode == 9 || //tab
        charCode == 46 || //del
        charCode == 36 || //home
        charCode == 35 || //end
        charCode == 37 || //<- 화살표
        charCode == 39) //-> 화살표
    {             
        return true; 
    }

    return false;
}

//숫자 한글변환
function numberToKorean(val, plusZero) {
    if (val == "0") {
        return "영";
    }
    if (val.substr(0, 1) == "0") {
        return "0으로 시작하는 숫자입니다.";
    }
    if (val.indexOf(".") > -1) {
        return "실수는 지원하지 않습니다.";
    }

    var num = ""; 
    var won = new Array();
    re = /[^0-9]+/g;
    num = val.replace(/,/g, '') +plusZero;

    if (num.length > 44) {
        return "숫자 44자까지 변환 가능합니다.";
    }
    if (re.exec(num)) {
        return "숫자만 입력가능합니다.";
    }

    var price_unit0 = new Array("", "일", "이", "삼", "사", "오", "육", "칠", "팔", "구");
    var price_unit1 = new Array("", "십", "백", "천");
    var price_unit2 = new Array("", "만", "억", "조", "경", "해", "시", "양", "구", "간", "정");
    
    for (i = num.length - 1; i >= 0; i--) {
        won[i] = price_unit0[num.substr(num.length - 1 - i, 1)];
        if (i > 0 && won[i] != "") { won[i] += price_unit1[i % 4]; }
        if (i % 4 == 0) { won[i] += price_unit2[(i / 4)]; }
    }
    for (i = num.length - 1; i >= 0; i--) {
        if (won[i].length == 2) { won[i - i % 4] += "-"; }
        if (won[i].length == 1 && i > 0) { won[i] = ""; }
        if (i % 4 != 0) { won[i] = won[i].replace("일", ""); }
    }

    won = won.reverse().join("").replace(/-+/g, "");

    if (won.length > 0) {
        won = won + '원';
    }
    
    return won;
}

//동영상 다시보기
function repeat(embed) {
    if (embed != null) {
        embed.stop();
        embed.play();
    }
}

function CheckOpenBox() {
    $(':radio').each(function () {
        var labelVariable = $(this).attr('Variable') + '_op' + $(this).attr('value');
        var clickID = $(this).attr('clickID');
        
        if ($(this).prop('checked')) {
            $('label[variable="' + labelVariable + '"]').show();
            $('td[clickID="' + clickID + '"],div[clickID="' + clickID + '"]').css("background-color", "#E0FFF0");
        }
        else {
            $('label[variable="' + labelVariable + '"]').hide();
            $('td[clickID="' + clickID + '"],div[clickID="' + clickID + '"]').css("background-color", "transparent");
        }
    });

    $(':checkbox').each(function () {
        var labelVariable = $(this).attr('rankingVariable') + '_op' + $(this).attr('value');
        var clickID = $(this).attr('clickID');

        if ($(this).prop('checked')) {
            $('label[variable="' + labelVariable + '"]').show();
            $('td[clickID="' + clickID + '"],div[clickID="' + clickID + '"]').css("background-color", "#E0FFF0");
        }
        else {
            $('label[variable="' + labelVariable + '"]').hide();
            $('td[clickID="' + clickID + '"],div[clickID="' + clickID + '"]').css("background-color", "transparent");
        }
    });
}

function CheckRanking(rankingVariable, rankingValue, isRanking) {
    $('label[id^="Rank_' + rankingVariable + '"]').text('');
    $(':checkbox[rankingVariable="' + rankingVariable + '"]').attr('ranking', '0');

    if (rankingValue != null) {
        var ranking = rankingValue.split(',');
        $.each(ranking, function (index, code) {
            //                랜덤색상
            //                var letters = '0123456789ABCDEF'.split('');
            //                var color = '#';

            //                for (var i = 0; i < 6; i++) {
            //                    color += letters[Math.round(Math.random() * 15)];
            //                }

            //                color = 'color:' + color + '; font-weight:bold;';

            color = 'color:red; font-weight:bold;';

            if (isRanking == 'True') {
                $('label[id="Rank_' + rankingVariable + '_' + code + '"]').text((index + 1) + ' 순위');
                $('label[id="Rank_' + rankingVariable + '_' + code + '"]').attr('style', color);
            }

            $(':checkbox[rankingVariable="' + rankingVariable + '"][value="' + code + '"]').attr('ranking', (index + 1));
        });
    }
}

function CheckStepByStep() {
    $('tr[stepbystep!="0"]').hide();
    $('tr:not([stepbystep])').show();
}

//이미지 맵용
//$('img[usemap]').maphilight({
//    fill: true,
//    fillColor: 'E16350',
//    fillOpacity: 0.2,
//    strokeColor: 'ff0000',
//    strokeOpacity: 1,
//    strokeWidth: 3
//});

$('area').click(function () {
    if ($(this).attr('isSingle') == 'true') {
        ResetArea();
    }

    var data = $(this).mouseout().data('maphilight') || {};
    data.alwaysOn = !data.alwaysOn;
    $(this).data('maphilight', data).trigger('alwaysOn.maphilight');
    $(this).prop('checked', data.alwaysOn);

    if ($(this).attr('isSingle') != 'true') {
        var variableType = 'rankingVariable';
        var rankingVariable = $(this).attr('rankingVariable');

        var rankingValue = ',' + $("#RankingValue_" + rankingVariable).val() + ',';
        var checkCode = $(this).attr('CodeLabel');

        if ($(this).prop('checked')) {
            if (rankingValue == '') {
                rankingValue = ',' + $(this).attr('CodeLabel') + ',';
            }
            else {
                rankingValue = rankingValue + ',' + $(this).attr('CodeLabel') + ',';
            }
        }
        else {
            rankingValue = rankingValue.replace(',' + $(this).attr('CodeLabel') + ',', ',');
        }

        //앞뒤 트림
        rankingValue = rankingValue.replace(/,,/g, ',');
        rankingValue = rankingValue.replace(/,$/g, '');
        rankingValue = rankingValue.replace(/^,/g, '');
        rankingValue = rankingValue.replace(/^,/g, '');

        $("#RankingValue_" + rankingVariable).val(rankingValue);
    }
});

function ResetArea() {
    $('area').each(function () {
        var data = $(this).mouseout().data('maphilight') || {};
        data.alwaysOn = false;
        $(this).data('maphilight', data).trigger('alwaysOn.maphilight');
        $(this).prop('checked', data.alwaysOn);
    });
}

$('td[clickID],label[clickID],div[clickID]').click(function () {
    if ($(':radio[clickID="' + $(this).attr('clickID') + '"]').is(':enabled')) {
        $(':radio[clickID="' + $(this).attr('clickID') + '"]').prop('checked', true).triggerHandler("click");
    }

    if ($(':checkbox[clickID="' + $(this).attr('clickID') + '"]').is(':enabled') && checkboxClickType == 1) {
        $(':checkbox[clickID="' + $(this).attr('clickID') + '"]').prop('checked', !$(':checkbox[clickID="' + $(this).attr('clickID') + '"]').prop('checked')).triggerHandler("click");
    }

    checkboxClickType = 1;
});

$("#btnClose").click(function () {
    window.opener.Refresh();
    self.close();
});