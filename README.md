# OpenJ2J
## Description
OpenJ2J is a open-source implementation of J2J format.

## Structure
```
J2J 포맷 변조 방식 - 한국어(Korean)

A. 상수
 * 블록 사이즈(blockSize) : 10,240,000Bytes
 
B. 블록 갯수 산출
 1. 파일 크기를 구한다. => fileSize(Bytes)
 2. blockSize를 (fileSize * 100)으로 나눈 몫을 구한다. => blockCount
 3. 만약 blockCount가 0이면 1로 변경한다.(blockCount는 반드시 1 이상이어야 한다.)

C. IV(Initialization Vector) 생성
[공통]
 1. blockSize 크기의 바이트 배열을 생성한다. => IV
 2. IV에 0x00~0xFF 반복해서 채운다.
[암호화 시]
 3. 비밀번호 평문을 UTF-8 바이트 배열로 변환한다. => PasswordBytes(PB)
 4. IV를 [정상] [변조(정상+PB(0))] [정상] [변조(정상+PB(1))] ... [정상] [변조(정상+PB(n))] [정상] 형태로 반복해서 변조한다.
 (※주의 : 정상 바이트에 PB 바이트를 더하는 방식으로 변조되며, 오버플로우가 발생한다는 전제 하에 IV를 생성하기 때문에 오버플로우를 방지하면 안됨.)
 
D. 파일 상단부 변조
 1. 파일을 오프셋이 0인 부분에서부터 blockSize만큼 읽어온다.
 2. 읽어온 바이트들을 IV의 바이트들과 XOR 연산한 후 IV에 저장한다.
 4. 오프셋을 다시 0으로 바꾸고, IV 값을 파일에 저장한다.
 5. 일련의 과정을 하단 블록들에도 반복한다.(blockCount 만큼)
 
E. 파일 하단부 변조
 1. 오프셋을 fileSize - (blockSize * blockCount)로 지정하고 blockSize만큼 읽어온다.
 2. 읽어온 바이트들을 IV의 바이트들과 XOR 연산한 후 IV에 저장한다.
 3. 오프셋을 fileSize - (blockSize * blockCount)로 재지정하고, IV 값을 파일에 저장한다.
 4. 일련의 과정을 하단 블록들에도 반복한다.(blockCount 만큼)
 
F. 파일 헤더 저장
 * 파일 최하단부에 아래 형식의 헤더를 저장한다.
 1. 0~7 : 0x00
 2. 8 : blockCount
 3. 9~15 : 0x00
 4. 16~23 : CRC32 해시(패딩 방식 : CRC Bytes -> UTF-8 String -> UTF-8 Bytes)
 5. 24~31 : "L3000009"

```
