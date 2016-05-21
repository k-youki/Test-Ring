#import "CRStatusReceiver.h"

extern "C"{
    CRStatusReceiver *receiver;
    void _StartRing();
    void _EndRing();
    void _QuaternionMode ();
    void _GestureMode ();
    typedef void (*RingQuatStatusCallback)(BOOL ringConnectStatus,float x, float y, float z, float w);
    void _GetRingQuatStatus(RingQuatStatusCallback callback);
    CRQuaternion prevQuat = {0,0,0,0};
    typedef void (*TouchStatusCallback)(BOOL touchStatus);
    void _GetTouchStatus (TouchStatusCallback callback);
    typedef void (*RingGestureStatusCallback)(BOOL ringConnectStatus, const char *ReceiveGesture);
    void _GetRingGestureStatus(RingGestureStatusCallback callback);
    
    float diffRemit = 0.01f;
    BOOL diffCheck (CRQuaternion currentQ, CRQuaternion prevQ);
}

//RingApp接続処理
void _StartRing()
{
    if (!receiver) {
        receiver = [[CRStatusReceiver alloc] init];
    }
    
    [receiver startRing];
}

void _EndRing()
{
    if (!receiver) {
        receiver = [[CRStatusReceiver alloc] init];
    }
    
    [receiver endRing];
}

void _QuaternionMode (){
    if (!receiver) {
        receiver = [[CRStatusReceiver alloc] init];
    }
    [receiver ChangeQuaternionMode];
}

void _GestureMode (){
    if (!receiver) {
        receiver = [[CRStatusReceiver alloc] init];
    }
    [receiver ChangeGestureMode];
}

void _GetRingGestureStatus(RingGestureStatusCallback callback)
{
    BOOL connectStatus = [receiver getRingStatus];
    NSString *gestureName = [receiver getGestureStatus];
    if(![gestureName isEqualToString:@"none"]){
         (callback)(connectStatus,[gestureName UTF8String]);
     }
}

void _GetRingQuatStatus(RingQuatStatusCallback callback)
{
    //Ringの情報(quaternion,接続状態)を取得
    float x = [receiver getQuat].x;
    float y = [receiver getQuat].y;
    float z = [receiver getQuat].z;
    float w = [receiver getQuat].w;
    BOOL connectStatus = [receiver getRingStatus];
    
    //送信回数を最小限にする
    if (diffCheck([receiver getQuat],prevQuat)){
        
        //RingQuatReceiver.csにコールバックを返す
        (callback)(connectStatus,x,y,z,w);
        
    }
    prevQuat = [receiver getQuat];
    
    NSLog(@"_GetRingStatus = %f %f %f %f", x,y,z,w);
}

void _GetTouchStatus (TouchStatusCallback callback){
    int touchStatus = [receiver getTouchStatus];
    BOOL touching = false;
    if (touchStatus == 1) {
        touching = true;
    }
    (callback)(touching);
}

BOOL diffCheck (CRQuaternion currentQ, CRQuaternion prevQ){
    int resetCount = 0;
    if (currentQ.x - prevQ.x < diffRemit && currentQ.x - prevQ.x > -diffRemit)
        resetCount += 1;
    
    if (currentQ.y - prevQ.y < diffRemit && currentQ.y - prevQ.y > -diffRemit)
        resetCount += 1;
    
    if (currentQ.z - prevQ.z < diffRemit && currentQ.z - prevQ.z > -diffRemit)
        resetCount += 1;
    
    if (currentQ.w - prevQ.w < diffRemit && currentQ.w - prevQ.w > -diffRemit)
        resetCount += 1;
    
    if (resetCount == 4) {
        NSLog(@"!diffCheck");
        return false;
    }else{
        return true;
    }
    
}
