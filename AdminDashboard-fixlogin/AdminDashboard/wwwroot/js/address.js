// address.js
const provinceApi = 'https://provinces.open-api.vn/api/p/';
const districtApi = 'https://provinces.open-api.vn/api/d/';
const wardApi = 'https://provinces.open-api.vn/api/w/';

async function loadProvinces() {
    const response = await fetch(provinceApi);
    const provinces = await response.json();
    const provinceSelect = document.getElementById('province');
    provinces.forEach(province => {
        const option = document.createElement('option');
        option.value = province.code;
        option.textContent = province.name;
        provinceSelect.appendChild(option);
    });
}

async function loadDistricts(provinceCode) {
    const response = await fetch(`${provinceApi}${provinceCode}?depth=2`);
    const province = await response.json();
    const districtSelect = document.getElementById('district');
    districtSelect.innerHTML = '<option value="">Chọn Quận/Huyện</option>';
    districtSelect.disabled = false;
    province.districts.forEach(district => {
        const option = document.createElement('option');
        option.value = district.code;
        option.textContent = district.name;
        districtSelect.appendChild(option);
    });
    const wardSelect = document.getElementById('ward');
    wardSelect.innerHTML = '<option value="">Chọn Phường/Xã</option>';
    wardSelect.disabled = true;
}

async function loadWards(districtCode) {
    const response = await fetch(`${districtApi}${districtCode}?depth=2`);
    const district = await response.json();
    const wardSelect = document.getElementById('ward');
    wardSelect.innerHTML = '<option value="">Chọn Phường/Xã</option>';
    wardSelect.disabled = false;
    district.wards.forEach(ward => {
        const option = document.createElement('option');
        option.value = ward.code;
        option.textContent = ward.name;
        wardSelect.appendChild(option);
    });
}

function combineAddress() {
    const provinceSelect = document.getElementById('province');
    const districtSelect = document.getElementById('district');
    const wardSelect = document.getElementById('ward');
    const detailAddress = document.getElementById('detailAddress').value;

    const provinceText = provinceSelect.options[provinceSelect.selectedIndex]?.textContent || '';
    const districtText = districtSelect.options[districtSelect.selectedIndex]?.textContent || '';
    const wardText = wardSelect.options[wardSelect.selectedIndex]?.textContent || '';

    const fullAddress = [detailAddress, wardText, districtText, provinceText].filter(part => part).join(', ');
    document.getElementById('fullAddress').value = fullAddress;
}

async function fillAddressFromString(fullAddress) {
    const provinceResponse = await fetch(provinceApi);
    const provinces = await provinceResponse.json();

    let selectedProvince = null;
    for (const province of provinces) {
        if (fullAddress.includes(province.name)) {
            selectedProvince = province;
            break;
        }
    }

    if (selectedProvince) {
        document.getElementById('province').value = selectedProvince.code;
        await loadDistricts(selectedProvince.code);

        const districtResponse = await fetch(`${provinceApi}${selectedProvince.code}?depth=2`);
        const provinceData = await districtResponse.json();
        let selectedDistrict = null;
        for (const district of provinceData.districts) {
            if (fullAddress.includes(district.name)) {
                selectedDistrict = district;
                break;
            }
        }

        if (selectedDistrict) {
            document.getElementById('district').value = selectedDistrict.code;
            await loadWards(selectedDistrict.code);

            const wardResponse = await fetch(`${districtApi}${selectedDistrict.code}?depth=2`);
            const districtData = await wardResponse.json();
            let selectedWard = null;
            for (const ward of districtData.wards) {
                if (fullAddress.includes(ward.name)) {
                    selectedWard = ward;
                    break;
                }
            }

            if (selectedWard) {
                document.getElementById('ward').value = selectedWard.code;
            }
        }
    }

    let detail = fullAddress;
    if (selectedProvince) {
        detail = detail.replace(selectedProvince.name, '').trim();
    }
    if (selectedDistrict) {
        detail = detail.replace(selectedDistrict.name, '').trim();
    }
    if (selectedWard) {
        detail = detail.replace(selectedWard.name, '').trim();
    }
    detail = detail.replace(/,+/g, ',').trim();
    if (detail.endsWith(',')) {
        detail = detail.slice(0, -1).trim();
    }

    document.getElementById('detailAddress').value = detail;
    combineAddress();
}

function initAddressEvents() {
    document.getElementById('province').addEventListener('change', function () {
        if (this.value) {
            loadDistricts(this.value);
        } else {
            document.getElementById('district').disabled = true;
            document.getElementById('ward').disabled = true;
        }
        combineAddress();
    });

    document.getElementById('district').addEventListener('change', function () {
        if (this.value) {
            loadWards(this.value);
        } else {
            document.getElementById('ward').disabled = true;
        }
        combineAddress();
    });

    document.getElementById('ward').addEventListener('change', combineAddress);
    document.getElementById('detailAddress').addEventListener('input', combineAddress);
}

// Hàm khởi tạo cho trang Create
function initAddressForCreate() {
    loadProvinces();
    initAddressEvents();
}

// Hàm khởi tạo cho trang Edit
function initAddressForEdit() {
    loadProvinces().then(() => {
        const fullAddress = document.getElementById('fullAddress').value;
        if (fullAddress) {
            fillAddressFromString(fullAddress);
        }
        initAddressEvents();
    });
}