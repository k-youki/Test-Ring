#import "CRStatusReceiver.h"

@interface CRStatusReceiver ()

@end


@implementation CRStatusReceiver{
    Quaternion currentQuat;
    BOOL connectStatus;
    int touchingStatus;
    int currentRingMode;
    bool didReceiveNewGesture;
    NSString *ReceivedGestureName;
}

- (id)init {
    self = [super init];
    if (self != nil) {
        currentQuat[0] = 1.0f;  //w
        currentQuat[1] = 0;     //x
        currentQuat[2] = 0;     //y
        currentQuat[3] = 0;     //z
        connectStatus = false;
        touchingStatus = 0;
        currentRingMode = CRRingModeGesture;
        didReceiveNewGesture = false;
        ReceivedGestureName = @"none";
    }
    return self;
}

-(void) startRing {
    //
    // Create an instance
    //
    if(!self.ringApp){
        NSLog(@"CRReceiver: startRing");
        
        self.ringApp = [[CRApplication alloc] initWithDelegate:self background:NO];
        
        NSDictionary *gestures = @{ @"CIRCLE" : CR_POINTS_CIRCLE,
                                    @"TRIANGLE" : CR_POINTS_TRIANGLE,
                                    @"HEART" : CR_POINTS_HEART,
                                    @"PIGTALE" : CR_POINTS_PIGTAIL,
                                    @"UP" :  CR_POINTS_UP,
                                    @"DOWN" : CR_POINTS_DOWN,
                                    @"LEFT" : CR_POINTS_LEFT,
                                    @"RIGHT" : CR_POINTS_RIGHT};
        if (![[_ringApp installedGestureIdentifiers] count]) {
            NSError *error;
            if (![_ringApp installGestures:gestures error:&error]) {
                NSLog(@"%@", error);
                return;
            }
        }
        [self.ringApp setActiveGestureIdentifiers:@[@"CIRCLE",
                                                    @"TRIANGLE",
                                                    @"HEART",
                                                    @"PIGTALE",
                                                    @"UP",
                                                    @"DOWN",
                                                    @"LEFT",
                                                    @"RIGHT"]];
        
        [self.ringApp start];
    }
}



- (void)endRing {
    self.ringApp = nil;
}

-(void)ChangeGestureMode{
    NSLog(@"%s", __FUNCTION__);
    [self.ringApp setRingMode:CRRingModeGesture];
    currentRingMode = CRRingModeGesture;
}

-(void)ChangeQuaternionMode{
    NSLog(@"%s", __FUNCTION__);
    [self.ringApp setRingMode:CRRingModeQuaternion];
    currentRingMode = CRRingModeQuaternion;
}


#pragma mark - CRApplicationDelegate

- (void)deviceDidDisconnect {
    NSLog(@"%s", __FUNCTION__);
    connectStatus = false;
}

- (void)deviceDidInitialize {
    NSLog(@"%s", __FUNCTION__);
    if(currentRingMode == CRRingModeQuaternion){
        [self.ringApp setRingMode:CRRingModeQuaternion];
    }
}

- (void)didReceiveEvent:(CRRingEvent)event {
    NSLog(@"%s", __FUNCTION__);
    if (event == CRRingEventTap) {
        touchingStatus = 1;
    }else if(event == CRRingEventLongPress){
        if(currentRingMode == CRRingModeQuaternion)
            [self.ringApp setRingMode:CRRingModeQuaternion];
    }
}

- (void)didReceiveGesture:(NSString *)identifier {
    NSLog(@"%s %@", __FUNCTION__, identifier);
    didReceiveNewGesture = true;
    if(identifier){
        ReceivedGestureName = identifier;
    }else{
        ReceivedGestureName = @"NOTFOUND";
    }
}

- (void)didReceiveQuaternion:(CRQuaternion)quaternion {
    NSLog(@"%s", __FUNCTION__);
//    Quaternion qs = { (float)0.6, (float)0.0000, (float)0.0000, -(float)0.27};
    Quaternion currq = {(float)quaternion.w,(float)quaternion.x,(float)quaternion.y,(float)quaternion.z};
    
//    Quaternion conjq;
//    quat_conjugated(currq, conjq);
//    
//    Quaternion  resultq;
//    quat_multiple(conjq, qs, resultq);
//    
    NSLog(@"didReceiveQuaternion = %f %f %f %f", quaternion.x,quaternion.y,quaternion.z,quaternion.w);
    currentQuat[0] = currq[0];
    currentQuat[1] = currq[1];
    currentQuat[2] = currq[2];
    currentQuat[3] = currq[3];
    connectStatus = true;
}

- (void)didReceivePoint:(CGPoint)point {
    NSLog(@"%s", __FUNCTION__);
}

//private

- (NSString*)getGestureStatus{
    if(didReceiveNewGesture){
        didReceiveNewGesture = false;
        return ReceivedGestureName;
    }else{
        return @"none";
    }
}

- (CRQuaternion)getQuat{
    CRQuaternion q;
    q.w = currentQuat[0];
    q.x = currentQuat[1];
    q.y = currentQuat[2];
    q.z = currentQuat[3];
    return q;
}

- (BOOL)getRingStatus{
    return connectStatus;
}

- (int)getTouchStatus{
    int currentStatus = touchingStatus;
    touchingStatus = 0;
    return currentStatus;
}

- (void) copyQuaternion:(Quaternion)dest q:(CRQuaternion) src {
    dest[0] = src.w;
    dest[1] = src.x;
    dest[2] = src.y;
    dest[3] = src.z;
}

void quat_conjugated(Quaternion quat, Quaternion output)
{
    output[0] =  quat[0];
    output[1] = -quat[1];
    output[2] = -quat[2];
    output[3] = -quat[3];
    return;
}

void quat_multiple(Quaternion quat1, Quaternion quat2, Quaternion output)
{
    output[0] = (quat1[0] * quat2[0] - quat1[1] * quat2[1] - quat1[2] * quat2[2] - quat1[3] * quat2[3]);
    output[1] = (quat1[0] * quat2[1] + quat1[1] * quat2[0] + quat1[2] * quat2[3] - quat1[3] * quat2[2]);
    output[2] = (quat1[0] * quat2[2] - quat1[1] * quat2[3] + quat1[2] * quat2[0] + quat1[3] * quat2[1]);
    output[3] = (quat1[0] * quat2[3] + quat1[1] * quat2[2] - quat1[2] * quat2[1] + quat1[3] * quat2[0]);
    return;
}


@end
