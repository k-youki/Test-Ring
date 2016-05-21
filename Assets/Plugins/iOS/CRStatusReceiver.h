#import <Foundation/Foundation.h>
#import <CoreRing/CoreRing.h>

@interface CRStatusReceiver : NSObject <CRApplicationDelegate>

@property (nonatomic, strong) CRApplication *ringApp;

- (void)startRing;
- (void)endRing;
-(void)ChangeGestureMode;
-(void)ChangeQuaternionMode;
- (id)init;
- (NSString*)getGestureStatus;
- (CRQuaternion)getQuat;
- (BOOL)getRingStatus;
- (int)getTouchStatus;

typedef float  Quaternion[4];
@end
