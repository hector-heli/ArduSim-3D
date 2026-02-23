float lastData=0, data =0;
float elapsedTime, lastElapsedTime;

void setup() {
  Serial.begin(57600);
  for(int i=2; i<20;i++) pinMode(i, INPUT);

}

void loop() {
  for (int i=2; i<20; i++){
    data += digitalRead(i)*(pow(2, i));
    //delay(1);
  }

    elapsedTime = (micros()-lastElapsedTime)/1000;
    if(data!=lastData) {
      Serial.print(elapsedTime,2);
      Serial.print("    ");
      Serial.println(data,0);
      lastData = data;
    }
    lastElapsedTime = micros();
  data = 0;

}
