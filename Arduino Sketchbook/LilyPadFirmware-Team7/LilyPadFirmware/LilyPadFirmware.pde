
#include <MatrixMath.h>
#include <NewSoftSerial.h>

boolean elbowFlexTest = false;
boolean wristFlexTest = false;
boolean wristAbTest = false;
boolean rotTest = false;

boolean feedback = false;
boolean moving = false;

float prevAngle;
float targetAngleDifference = 90.0;
float startAngle = -1.0;

MatrixMath math;

NewSoftSerial bicepSerial(5,6);
NewSoftSerial forearmSerial(7,6);
NewSoftSerial wristSerial(8,6);

boolean getSensorBoolean;

float elbowAngle;
float wristflexAngle;
float wristabAngle;
float rotAngle;

float roll_b, pitch_b, yaw_b;
float roll_f, pitch_f, yaw_f;
float roll_w, pitch_w, yaw_w;

float Up_Vector[3];
float Forward_Vector[3];
float Side_Vector[3];

float Rotated_Vector[3];
float Rotated_Vector2[3];

void setup()
{
  Serial.begin(19200);
  bicepSerial.begin(28800);
  forearmSerial.begin(28800);
  wristSerial.begin(28800);
  
  Up_Vector[0] = 0.0;
  Up_Vector[1] = 0.0;
  Up_Vector[2] = 1.0;
  Forward_Vector[0] = 1.0;
  Forward_Vector[1] = 0.0;
  Forward_Vector[2] = 0.0;
  Side_Vector[0] = 0.0;
  Side_Vector[1] = 1.0;
  Side_Vector[2] = 0.0;
  
  pinMode(9,OUTPUT);
  pinMode(10,OUTPUT);
  pinMode(4,INPUT);
  //digitalWrite(9,HIGH);
  //delay(2000);
  //digitalWrite(9,LOW);
}

void loop()
{
  getSensorBoolean = false;
  do { 
    getSensorBoolean = getSensorData(&bicepSerial, &roll_b, &pitch_b, &yaw_b);
  } while(!getSensorBoolean);
  do { 
    getSensorBoolean = getSensorData(&forearmSerial, &roll_f, &pitch_f, &yaw_f);
  } while(!getSensorBoolean);
  do { 
    getSensorBoolean = getSensorData(&wristSerial, &roll_w, &pitch_w, &yaw_w);
  } while(!getSensorBoolean);
  
  computeVector((float*)Forward_Vector, (float*)Rotated_Vector, roll_b, pitch_b, yaw_b);
  computeVector((float*)Forward_Vector, (float*)Rotated_Vector2, roll_f, pitch_f, yaw_f);
  elbowAngle = getAngleBetween((float*)Rotated_Vector, (float*)Rotated_Vector2);
  
  //computeVector((float*)Up_Vector, (float*)Rotated_Vector, roll_w, pitch_w, yaw_w);
  //computeVector((float*)Forward_Vector, (float*)Rotated_Vector2, roll_f, pitch_f, yaw_f);
  //wristflexAngle = getAngleBetween((float*)Rotated_Vector, (float*)Rotated_Vector2);
  
  //computeVector((float*)Forward_Vector, (float*)Rotated_Vector, roll_w, pitch_w, yaw_w);
  //computeVector((float*)Forward_Vector, (float*)Rotated_Vector2, roll_f, pitch_f, yaw_f);
  //wristabAngle = getAngleBetween((float*)Rotated_Vector, (float*)Rotated_Vector2);
  
  //computeVector((float*)Side_Vector, (float*)Rotated_Vector, roll_f, pitch_f, yaw_f);
  //computeVector((float*)Side_Vector, (float*)Rotated_Vector2, roll_b, pitch_b, yaw_b);
  //rotAngle = getAngleBetween((float*)Rotated_Vector, (float*)Rotated_Vector2);
  
  /*Serial.print("Angle:");
  Serial.print(elbowAngle);
  Serial.println();*/
  
  Serial.print("W");
  Serial.print(roll_w);
  Serial.print(",");
  Serial.print(pitch_w);
  Serial.print(",");
  Serial.print(yaw_w);
  Serial.println();
  
  Serial.print("F");
  Serial.print(roll_f);
  Serial.print(",");
  Serial.print(pitch_f);
  Serial.print(",");
  Serial.print(yaw_f);
  Serial.println();

  Serial.print("B");
  Serial.print(roll_b);
  Serial.print(",");
  Serial.print(pitch_b);
  Serial.print(",");
  Serial.print(yaw_b);
  Serial.println();



  
  if(elbowFlexTest)
  {
    if(startAngle < 0)
    {
      startAngle = elbowAngle;
    }
    if(feedback)
    {
        if((abs(startAngle - elbowAngle) < (targetAngleDifference + 10)) && (abs(startAngle - elbowAngle) > (targetAngleDifference - 10)))
        {
          digitalWrite(9,LOW);
          digitalWrite(10,LOW);
          Serial.println("---CORRECT---");
        }
        else if(abs(startAngle - elbowAngle) < (targetAngleDifference + 10))
        {
          digitalWrite(10,LOW);
          digitalWrite(9,HIGH);
          Serial.println("---FLEX---");
        }
        else
        {
          digitalWrite(9,LOW);
          digitalWrite(10,HIGH);
          Serial.println("---EXTEND---");
        }
    }
    if(moving && abs(prevAngle - elbowAngle) < 10)
    {
      Serial.println("---STOP---");
      moving = false;
      feedback = true;
    }
    if((feedback != true) && (moving != true) && (abs(startAngle - elbowAngle) > 10) && (abs(startAngle - elbowAngle) < 50))
    {
      Serial.println("---MOVING---");
      moving = true;
    }
    prevAngle = elbowAngle;
  }
}

bool getSensorData(NewSoftSerial *newSerial, float *roll_float, float *pitch_float, float *yaw_float)
{
  if ((*newSerial).available() && ((char)(*newSerial).read())=='!' && ((char)(*newSerial).read())=='!' && ((char)(*newSerial).read())=='!') {
        char temp;
        String roll="", pitch="", yaw="";

        // Check if valid feedback line ("!!!" in front), and if so, get roll, pitch and yaw from buffer
        (*newSerial).read(); // Get rid of very first comma after "!!!"

        // Get Roll
//        Serial.println("Getting roll");
        temp = (char)(*newSerial).read();
        do
        {
          roll += temp;
        }
        while((temp = ((char)(*newSerial).read())) != ',');
        
        // Get Pitch
  //      Serial.println("Getting pitch");
        temp = (char)(*newSerial).read();
        do
        {
          pitch += temp;
        }
        while((temp = ((char)(*newSerial).read())) != ',');
        
        // Get Yaw
    //    Serial.println("Getting yaw");
        temp = (char)(*newSerial).read();
        do
        {
          yaw += temp;
        }
        while((temp = ((char)(*newSerial).read())) != ',');
     
        // Convert roll, pitch and yaw strings into integers
        char roll_char_array[roll.length() + 1];
        roll.toCharArray(roll_char_array, sizeof(roll_char_array));
        *roll_float = atof(roll_char_array);

        char pitch_char_array[pitch.length() + 1];
        pitch.toCharArray(pitch_char_array, sizeof(pitch_char_array));
        *pitch_float = atof(pitch_char_array);

        char yaw_char_array[yaw.length() + 1];
        yaw.toCharArray(yaw_char_array, sizeof(yaw_char_array));
        *yaw_float = atof(yaw_char_array);

        return true;        
  }
  else {
    return false;
  }
}

float getDotProduct(float* v1, float* v2) {
	return (v1)[0]*(v2)[0] + (v1)[1]*(v2)[1] + (v1)[2]*(v2)[2];
}

float getVectorMag(float* Current_Vector) {
	return sqrt((Current_Vector)[0]*(Current_Vector)[0] + (Current_Vector)[1]*(Current_Vector)[1] + (Current_Vector)[2]*(Current_Vector)[2]);
}

float getAngleBetween(float* v1, float* v2) {
	return acos(getDotProduct(v1, v2) / (getVectorMag(v1) * getVectorMag(v2))) * (180.0/PI);
}

void computeVector(float * directionVector, float * currentVector, float roll_float, float pitch_float, float yaw_float) {
  float R_x[3][3];
  float R_y[3][3];
  float R_z[3][3];
  float firstMatrixMult[3];
  float secondMatrixMult[3];

  R_x[0][0] = 1.0;
  R_x[0][1] = 0;                        
  R_x[0][2] = 0;
  R_x[1][0] = 0;   
  R_x[1][1] = cos((PI/180.0)*roll_float); 
  R_x[1][2] = (-1.0*sin((PI/180.0)*roll_float));
  R_x[2][0] = 0;   
  R_x[2][1] = sin((PI/180.0)*roll_float); 
  R_x[2][2] = cos((PI/180.0)*roll_float);

  R_y[0][0] = cos((PI/180.0)*pitch_float);        
  R_y[0][1] = 0;   
  R_y[0][2] = sin((PI/180.0)*pitch_float);
  R_y[1][0] = 0;                                
  R_y[1][1] = 1.0; 
  R_y[1][2] = 0;
  R_y[2][0] = (-1.0*sin((PI/180.0)*pitch_float)); 
  R_y[2][1] = 0;   
  R_y[2][2] = cos((PI/180.0)*pitch_float);

  R_z[0][0] = cos((PI/180.0)*yaw_float); 
  R_z[0][1] = (-1.0*sin((PI/180.0)*yaw_float)); 
  R_z[0][2] = 0;
  R_z[1][0] = sin((PI/180.0)*yaw_float); 
  R_z[1][1] = cos((PI/180.0)*yaw_float);        
  R_z[1][2] = 0;
  R_z[2][0] = 0;                       
  R_z[2][1] = 0;                              
  R_z[2][2] = 1;

  math.MatrixMult((float*)R_x,directionVector,3,3,1,(float*)firstMatrixMult);
  math.MatrixMult((float*)R_y,(float*)firstMatrixMult,3,3,1,(float*)secondMatrixMult);
  math.MatrixMult((float*)R_z,(float*)secondMatrixMult,3,3,1,currentVector);
}
