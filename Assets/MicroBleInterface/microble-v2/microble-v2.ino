#include "MMA8653.h"
#include <BLEPeripheral.h>

MMA8653 accel;
BLEPeripheral blePeripheral = BLEPeripheral();

// create service
BLEService accelService = BLEService("19b10010-e8f2-537e-4f6c-d104768a1214");
BLECharacteristic controlCharacteristic = BLECharacteristic("19b10011-e8f2-537e-4f6c-d104768a1214",
    BLEWrite, 64);
BLECharacteristic  accelCharacteristic = BLECharacteristic ("19b10012-e8f2-537e-4f6c-d104768a1214",
    BLERead | BLENotify, 64);

bool connectflg = false;
int timer;
int notifytime = 100;

const uint8_t max_cols = 9;
const uint8_t max_rows = 3;
const uint8_t cols[max_cols] = {3, 4, 10, 23, 24, 25, 9, 7, 6};
const uint8_t rows[max_rows] = {26, 27, 28};

typedef struct TPoint {
  uint8_t x;
  uint8_t y;
} POINT;

const POINT ledpos[5][5] =
{
  {{0, 0}, {3, 1}, {1, 0}, {4, 1}, {2, 0}},
  {{3, 2}, {4, 2}, {5, 2}, {6, 2}, {7, 2}},
  {{1, 1}, {8, 0}, {2, 1}, {8, 2}, {0, 1}},
  {{7, 0}, {6, 0}, {5, 0}, {4, 0}, {3, 0}},
  {{2, 2}, {6, 1}, {0, 2}, {5, 1}, {1, 2}}
};

void setup() {
  Serial.begin(115200);
  blePeripheral.setLocalName("micro:BLE");
  blePeripheral.setDeviceName("micro:BLE");

  blePeripheral.setAdvertisedServiceUuid(accelService.uuid());

  blePeripheral.addAttribute(accelService);
  blePeripheral.addAttribute(controlCharacteristic);
  blePeripheral.addAttribute(accelCharacteristic);

  blePeripheral.setEventHandler(BLEConnected, blePeripheralConnectHandler);
  blePeripheral.setEventHandler(BLEDisconnected, blePeripheralDisconnectHandler);

  // BLE init
  blePeripheral.begin();
  Serial.println(F("micro:BLE Accelerometer Peripheral"));

  accel.begin(false, 2); // 8-bit mode, 2g range

  led_init();
}

void loop() {
  blePeripheral.poll();

  if (connectflg) {
    if (controlCharacteristic.written()) {
      getContolCharacteristicValue();
    }

    timer ++;
    if (timer >= (notifytime / 10))
    {
      timer = 0;
      setAccelCharacteristicValue();
    }
  }
  else {
    Serial.println("not connect");
  }

  delay(30);
}

void led_init() {
  for (int i = 0; i < max_cols; i++) {
    pinMode(cols[i], OUTPUT);
    digitalWrite(cols[i], HIGH);
  }
  for (int i = 0; i < max_rows; i++) {
    pinMode(rows[i], OUTPUT);
    digitalWrite(rows[i], LOW);
  }
}

void led_pset(const uint8_t x, const uint8_t y, const uint8_t mode) {
  POINT position = ledpos[y][x];
  digitalWrite(cols[position.x], !mode);
  digitalWrite(rows[position.y], mode);
}

void getContolCharacteristicValue() {
  Serial.println("************** receive data! ******************");
  if (controlCharacteristic.value()) {
    byte row = controlCharacteristic.value()[0];
    byte col = controlCharacteristic.value()[1];
    Serial.println(row);
    Serial.println(col);

    led_pset( col, row, HIGH);
    delay(200);
    led_pset( col, row, LOW);
  }
}

void setAccelCharacteristicValue() {
  char acceldata[30];

  accel.update();
  float accelX = (float)accel.getX() * 0.0156;
  float accelY = (float)accel.getY() * 0.0156;
  float accelZ = (float)accel.getZ() * 0.0156;
  sprintf(acceldata, "accel - X: % 3.4f Y: % 3.4f Z: % 3.4f", accelX , accelY , accelZ );
  Serial.println(acceldata);

  sprintf(acceldata, " % d, % d, % d", (int)(accelX * 100000), (int)(accelY * 100000), (int)(accelZ * 100000));
  Serial.println(acceldata);
  //accelCharacteristic.setValue(accel);

  String saccel = String(acceldata);
  //Serial.println(saccel);
  byte sbyte[saccel.length()];
  saccel.getBytes(sbyte, saccel.length() + 1);
  accelCharacteristic.setValue(sbyte, saccel.length());
}

void blePeripheralConnectHandler(BLECentral& central) {
  connectflg = true;
  Serial.print(F("Connected event, central: "));
  Serial.println(central.address());
}

void blePeripheralDisconnectHandler(BLECentral& central) {
  connectflg = false;
  Serial.print(F("Disconnected event, central: "));
  Serial.println(central.address());
}
