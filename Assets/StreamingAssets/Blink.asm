
./Blink.hex:     file format ihex


Disassembly of section .sec1:

00000000 <.sec1>:
   0:	0c 94 5c 00 	jmp	0xb8	;  0xb8
   4:	0c 94 6e 00 	jmp	0xdc	;  0xdc
   8:	0c 94 6e 00 	jmp	0xdc	;  0xdc
   c:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  10:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  14:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  18:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  1c:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  20:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  24:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  28:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  2c:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  30:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  34:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  38:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  3c:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  40:	0c 94 36 01 	jmp	0x26c	;  0x2  git log --oneline --graph --all6c
  44:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  48:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  4c:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  50:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  54:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  58:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  5c:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  60:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  64:	0c 94 6e 00 	jmp	0xdc	;  0xdc
  68:	00 00       	nop
  6a:	00 08       	sbc	r0, r0
  6c:	00 02       	muls	r16, r16
  6e:	01 00       	.word	0x0001	; ????
  70:	00 03       	mulsu	r16, r16
  72:	04 07       	cpc	r16, r20
	...
  80:	25 00       	.word	0x0025	; ????
  82:	28 00       	.word	0x0028	; ????
  84:	2b 00       	.word	0x002b	; ????
  86:	00 00       	nop
  88:	00 00       	nop
  8a:	24 00       	.word	0x0024	; ????
  8c:	27 00       	.word	0x0027	; ????
  8e:	2a 00       	.word	0x002a	; ????
  90:	04 04       	cpc	r0, r4
  92:	04 04       	cpc	r0, r4
  94:	04 04       	cpc	r0, r4
  96:	04 04       	cpc	r0, r4
  98:	02 02       	muls	r16, r18
  9a:	02 02       	muls	r16, r18
  9c:	02 02       	muls	r16, r18
  9e:	03 03       	mulsu	r16, r19
  a0:	03 03       	mulsu	r16, r19
  a2:	03 03       	mulsu	r16, r19
  a4:	01 02       	muls	r16, r17
  a6:	04 08       	sbc	r0, r4
  a8:	10 20       	and	r1, r0
  aa:	40 80       	ld	r4, Z
  ac:	01 02       	muls	r16, r17
  ae:	04 08       	sbc	r0, r4
  b0:	10 20       	and	r1, r0
  b2:	01 02       	muls	r16, r17
  b4:	04 08       	sbc	r0, r4
  b6:	10 20       	and	r1, r0
  b8:	11 24       	eor	r1, r1
  ba:	1f be       	out	0x3f, r1	; 63
  bc:	cf ef       	ldi	r28, 0xFF	; 255
  be:	d8 e0       	ldi	r29, 0x08	; 8
  c0:	de bf       	out	0x3e, r29	; 62
  c2:	cd bf       	out	0x3d, r28	; 61
  c4:	21 e0       	ldi	r18, 0x01	; 1
  c6:	a0 e0       	ldi	r26, 0x00	; 0
  c8:	b1 e0       	ldi	r27, 0x01	; 1
  ca:	01 c0       	rjmp	.+2      	;  0xce
  cc:	1d 92       	st	X+, r1
  ce:	a9 30       	cpi	r26, 0x09	; 9
  d0:	b2 07       	cpc	r27, r18
  d2:	e1 f7       	brne	.-8      	;  0xcc
  d4:	0e 94 80 01 	call	0x300	;  0x300
  d8:	0c 94 27 02 	jmp	0x44e	;  0x44e
  dc:	0c 94 00 00 	jmp	0	;  0x0
  e0:	90 e0       	ldi	r25, 0x00	; 0
  e2:	fc 01       	movw	r30, r24
  e4:	ec 55       	subi	r30, 0x5C	; 92
  e6:	ff 4f       	sbci	r31, 0xFF	; 255
  e8:	24 91       	lpm	r18, Z
  ea:	80 57       	subi	r24, 0x70	; 112
  ec:	9f 4f       	sbci	r25, 0xFF	; 255
  ee:	fc 01       	movw	r30, r24
  f0:	84 91       	lpm	r24, Z
  f2:	88 23       	and	r24, r24
  f4:	99 f0       	breq	.+38     	;  0x11c
  f6:	90 e0       	ldi	r25, 0x00	; 0
  f8:	88 0f       	add	r24, r24
  fa:	99 1f       	adc	r25, r25
  fc:	fc 01       	movw	r30, r24
  fe:	ea 57       	subi	r30, 0x7A	; 122
 100:	ff 4f       	sbci	r31, 0xFF	; 255
 102:	a5 91       	lpm	r26, Z+
 104:	b4 91       	lpm	r27, Z
 106:	fc 01       	movw	r30, r24
 108:	e4 58       	subi	r30, 0x84	; 132
 10a:	ff 4f       	sbci	r31, 0xFF	; 255
 10c:	85 91       	lpm	r24, Z+
 10e:	94 91       	lpm	r25, Z
 110:	8f b7       	in	r24, 0x3f	; 63
 112:	f8 94       	cli
 114:	ec 91       	ld	r30, X
 116:	e2 2b       	or	r30, r18
 118:	ec 93       	st	X, r30
 11a:	8f bf       	out	0x3f, r24	; 63
 11c:	08 95       	ret
 11e:	90 e0       	ldi	r25, 0x00	; 0
 120:	fc 01       	movw	r30, r24
 122:	e8 59       	subi	r30, 0x98	; 152
 124:	ff 4f       	sbci	r31, 0xFF	; 255
 126:	24 91       	lpm	r18, Z
 128:	fc 01       	movw	r30, r24
 12a:	ec 55       	subi	r30, 0x5C	; 92
 12c:	ff 4f       	sbci	r31, 0xFF	; 255
 12e:	34 91       	lpm	r19, Z
 130:	fc 01       	movw	r30, r24
 132:	e0 57       	subi	r30, 0x70	; 112
 134:	ff 4f       	sbci	r31, 0xFF	; 255
 136:	e4 91       	lpm	r30, Z
 138:	ee 23       	and	r30, r30
 13a:	c9 f0       	breq	.+50     	;  0x16e
 13c:	22 23       	and	r18, r18
 13e:	39 f0       	breq	.+14     	;  0x14e
 140:	23 30       	cpi	r18, 0x03	; 3
 142:	01 f1       	breq	.+64     	;  0x184
 144:	a8 f4       	brcc	.+42     	;  0x170
 146:	21 30       	cpi	r18, 0x01	; 1
 148:	19 f1       	breq	.+70     	;  0x190
 14a:	22 30       	cpi	r18, 0x02	; 2
 14c:	29 f1       	breq	.+74     	;  0x198
 14e:	f0 e0       	ldi	r31, 0x00	; 0
 150:	ee 0f       	add	r30, r30
 152:	ff 1f       	adc	r31, r31
 154:	e4 58       	subi	r30, 0x84	; 132
 156:	ff 4f       	sbci	r31, 0xFF	; 255
 158:	a5 91       	lpm	r26, Z+
 15a:	b4 91       	lpm	r27, Z
 15c:	8f b7       	in	r24, 0x3f	; 63
 15e:	f8 94       	cli
 160:	ec 91       	ld	r30, X
 162:	61 11       	cpse	r22, r1
 164:	26 c0       	rjmp	.+76     	;  0x1b2
 166:	30 95       	com	r19
 168:	3e 23       	and	r19, r30
 16a:	3c 93       	st	X, r19
 16c:	8f bf       	out	0x3f, r24	; 63
 16e:	08 95       	ret
 170:	27 30       	cpi	r18, 0x07	; 7
 172:	a9 f0       	breq	.+42     	;  0x19e
 174:	28 30       	cpi	r18, 0x08	; 8
 176:	c9 f0       	breq	.+50     	;  0x1aa
 178:	24 30       	cpi	r18, 0x04	; 4
 17a:	49 f7       	brne	.-46     	;  0x14e
 17c:	80 91 80 00 	lds	r24, 0x0080	;  0x800080
 180:	8f 7d       	andi	r24, 0xDF	; 223
 182:	03 c0       	rjmp	.+6      	;  0x18a
 184:	80 91 80 00 	lds	r24, 0x0080	;  0x800080
 188:	8f 77       	andi	r24, 0x7F	; 127
 18a:	80 93 80 00 	sts	0x0080, r24	;  0x800080
 18e:	df cf       	rjmp	.-66     	;  0x14e
 190:	84 b5       	in	r24, 0x24	; 36
 192:	8f 77       	andi	r24, 0x7F	; 127
 194:	84 bd       	out	0x24, r24	; 36
 196:	db cf       	rjmp	.-74     	;  0x14e
 198:	84 b5       	in	r24, 0x24	; 36
 19a:	8f 7d       	andi	r24, 0xDF	; 223
 19c:	fb cf       	rjmp	.-10     	;  0x194
 19e:	80 91 b0 00 	lds	r24, 0x00B0	;  0x8000b0
 1a2:	8f 77       	andi	r24, 0x7F	; 127
 1a4:	80 93 b0 00 	sts	0x00B0, r24	;  0x8000b0
 1a8:	d2 cf       	rjmp	.-92     	;  0x14e
 1aa:	80 91 b0 00 	lds	r24, 0x00B0	;  0x8000b0
 1ae:	8f 7d       	andi	r24, 0xDF	; 223
 1b0:	f9 cf       	rjmp	.-14     	;  0x1a4
 1b2:	3e 2b       	or	r19, r30
 1b4:	da cf       	rjmp	.-76     	;  0x16a
 1b6:	3f b7       	in	r19, 0x3f	; 63
 1b8:	f8 94       	cli
 1ba:	80 91 05 01 	lds	r24, 0x0105	;  0x800105
 1be:	90 91 06 01 	lds	r25, 0x0106	;  0x800106
 1c2:	a0 91 07 01 	lds	r26, 0x0107	;  0x800107
 1c6:	b0 91 08 01 	lds	r27, 0x0108	;  0x800108
 1ca:	26 b5       	in	r18, 0x26	; 38
 1cc:	a8 9b       	sbis	0x15, 0	; 21
 1ce:	05 c0       	rjmp	.+10     	;  0x1da
 1d0:	2f 3f       	cpi	r18, 0xFF	; 255
 1d2:	19 f0       	breq	.+6      	;  0x1da
 1d4:	01 96       	adiw	r24, 0x01	; 1
 1d6:	a1 1d       	adc	r26, r1
 1d8:	b1 1d       	adc	r27, r1
 1da:	3f bf       	out	0x3f, r19	; 63
 1dc:	ba 2f       	mov	r27, r26
 1de:	a9 2f       	mov	r26, r25
 1e0:	98 2f       	mov	r25, r24
 1e2:	88 27       	eor	r24, r24
 1e4:	bc 01       	movw	r22, r24
 1e6:	cd 01       	movw	r24, r26
 1e8:	62 0f       	add	r22, r18
 1ea:	71 1d       	adc	r23, r1
 1ec:	81 1d       	adc	r24, r1
 1ee:	91 1d       	adc	r25, r1
 1f0:	42 e0       	ldi	r20, 0x02	; 2
 1f2:	66 0f       	add	r22, r22
 1f4:	77 1f       	adc	r23, r23
 1f6:	88 1f       	adc	r24, r24
 1f8:	99 1f       	adc	r25, r25
 1fa:	4a 95       	dec	r20
 1fc:	d1 f7       	brne	.-12     	;  0x1f2
 1fe:	08 95       	ret
 200:	8f 92       	push	r8
 202:	9f 92       	push	r9
 204:	af 92       	push	r10
 206:	bf 92       	push	r11
 208:	cf 92       	push	r12
 20a:	df 92       	push	r13
 20c:	ef 92       	push	r14
 20e:	ff 92       	push	r15
 210:	0e 94 db 00 	call	0x1b6	;  0x1b6
 214:	4b 01       	movw	r8, r22
 216:	5c 01       	movw	r10, r24
 218:	8c e2       	ldi	r24, 0x2C	; 44
 21a:	c8 2e       	mov	r12, r24
 21c:	dd 24       	eor	r13, r13
 21e:	d3 94       	inc	r13
 220:	e1 2c       	mov	r14, r1
 222:	f1 2c       	mov	r15, r1
 224:	0e 94 db 00 	call	0x1b6	;  0x1b6
 228:	68 19       	sub	r22, r8
 22a:	79 09       	sbc	r23, r9
 22c:	8a 09       	sbc	r24, r10
 22e:	9b 09       	sbc	r25, r11
 230:	68 3e       	cpi	r22, 0xE8	; 232
 232:	73 40       	sbci	r23, 0x03	; 3
 234:	81 05       	cpc	r24, r1
 236:	91 05       	cpc	r25, r1
 238:	a8 f3       	brcs	.-22     	;  0x224
 23a:	21 e0       	ldi	r18, 0x01	; 1
 23c:	c2 1a       	sub	r12, r18
 23e:	d1 08       	sbc	r13, r1
 240:	e1 08       	sbc	r14, r1
 242:	f1 08       	sbc	r15, r1
 244:	88 ee       	ldi	r24, 0xE8	; 232
 246:	88 0e       	add	r8, r24
 248:	83 e0       	ldi	r24, 0x03	; 3
 24a:	98 1e       	adc	r9, r24
 24c:	a1 1c       	adc	r10, r1
 24e:	b1 1c       	adc	r11, r1
 250:	c1 14       	cp	r12, r1
 252:	d1 04       	cpc	r13, r1
 254:	e1 04       	cpc	r14, r1
 256:	f1 04       	cpc	r15, r1
 258:	29 f7       	brne	.-54     	;  0x224
 25a:	ff 90       	pop	r15
 25c:	ef 90       	pop	r14
 25e:	df 90       	pop	r13
 260:	cf 90       	pop	r12
 262:	bf 90       	pop	r11
 264:	af 90       	pop	r10
 266:	9f 90       	pop	r9
 268:	8f 90       	pop	r8
 26a:	08 95       	ret
 26c:	1f 92       	push	r1
 26e:	0f 92       	push	r0
 270:	0f b6       	in	r0, 0x3f	; 63
 272:	0f 92       	push	r0
 274:	11 24       	eor	r1, r1
 276:	2f 93       	push	r18
 278:	3f 93       	push	r19
 27a:	8f 93       	push	r24
 27c:	9f 93       	push	r25
 27e:	af 93       	push	r26
 280:	bf 93       	push	r27
 282:	80 91 01 01 	lds	r24, 0x0101	;  0x800101
 286:	90 91 02 01 	lds	r25, 0x0102	;  0x800102
 28a:	a0 91 03 01 	lds	r26, 0x0103	;  0x800103
 28e:	b0 91 04 01 	lds	r27, 0x0104	;  0x800104
 292:	30 91 00 01 	lds	r19, 0x0100	;  0x800100
 296:	23 e0       	ldi	r18, 0x03	; 3
 298:	23 0f       	add	r18, r19
 29a:	2d 37       	cpi	r18, 0x7D	; 125
 29c:	58 f5       	brcc	.+86     	;  0x2f4
 29e:	01 96       	adiw	r24, 0x01	; 1
 2a0:	a1 1d       	adc	r26, r1
 2a2:	b1 1d       	adc	r27, r1
 2a4:	20 93 00 01 	sts	0x0100, r18	;  0x800100
 2a8:	80 93 01 01 	sts	0x0101, r24	;  0x800101
 2ac:	90 93 02 01 	sts	0x0102, r25	;  0x800102
 2b0:	a0 93 03 01 	sts	0x0103, r26	;  0x800103
 2b4:	b0 93 04 01 	sts	0x0104, r27	;  0x800104
 2b8:	80 91 05 01 	lds	r24, 0x0105	;  0x800105
 2bc:	90 91 06 01 	lds	r25, 0x0106	;  0x800106
 2c0:	a0 91 07 01 	lds	r26, 0x0107	;  0x800107
 2c4:	b0 91 08 01 	lds	r27, 0x0108	;  0x800108
 2c8:	01 96       	adiw	r24, 0x01	; 1
 2ca:	a1 1d       	adc	r26, r1
 2cc:	b1 1d       	adc	r27, r1
 2ce:	80 93 05 01 	sts	0x0105, r24	;  0x800105
 2d2:	90 93 06 01 	sts	0x0106, r25	;  0x800106
 2d6:	a0 93 07 01 	sts	0x0107, r26	;  0x800107
 2da:	b0 93 08 01 	sts	0x0108, r27	;  0x800108
 2de:	bf 91       	pop	r27
 2e0:	af 91       	pop	r26
 2e2:	9f 91       	pop	r25
 2e4:	8f 91       	pop	r24
 2e6:	3f 91       	pop	r19
 2e8:	2f 91       	pop	r18
 2ea:	0f 90       	pop	r0
 2ec:	0f be       	out	0x3f, r0	; 63
 2ee:	0f 90       	pop	r0
 2f0:	1f 90       	pop	r1
 2f2:	18 95       	reti
 2f4:	26 e8       	ldi	r18, 0x86	; 134
 2f6:	23 0f       	add	r18, r19
 2f8:	02 96       	adiw	r24, 0x02	; 2
 2fa:	a1 1d       	adc	r26, r1
 2fc:	b1 1d       	adc	r27, r1
 2fe:	d2 cf       	rjmp	.-92     	;  0x2a4
 300:	78 94       	sei
 302:	84 b5       	in	r24, 0x24	; 36
 304:	82 60       	ori	r24, 0x02	; 2
 306:	84 bd       	out	0x24, r24	; 36
 308:	84 b5       	in	r24, 0x24	; 36
 30a:	81 60       	ori	r24, 0x01	; 1
 30c:	84 bd       	out	0x24, r24	; 36
 30e:	85 b5       	in	r24, 0x25	; 37
 310:	82 60       	ori	r24, 0x02	; 2
 312:	85 bd       	out	0x25, r24	; 37
 314:	85 b5       	in	r24, 0x25	; 37
 316:	81 60       	ori	r24, 0x01	; 1
 318:	85 bd       	out	0x25, r24	; 37
 31a:	80 91 6e 00 	lds	r24, 0x006E	;  0x80006e
 31e:	81 60       	ori	r24, 0x01	; 1
 320:	80 93 6e 00 	sts	0x006E, r24	;  0x80006e
 324:	10 92 81 00 	sts	0x0081, r1	;  0x800081
 328:	80 91 81 00 	lds	r24, 0x0081	;  0x800081
 32c:	82 60       	ori	r24, 0x02	; 2
 32e:	80 93 81 00 	sts	0x0081, r24	;  0x800081
 332:	80 91 81 00 	lds	r24, 0x0081	;  0x800081
 336:	81 60       	ori	r24, 0x01	; 1
 338:	80 93 81 00 	sts	0x0081, r24	;  0x800081
 33c:	80 91 80 00 	lds	r24, 0x0080	;  0x800080
 340:	81 60       	ori	r24, 0x01	; 1
 342:	80 93 80 00 	sts	0x0080, r24	;  0x800080
 346:	80 91 b1 00 	lds	r24, 0x00B1	;  0x8000b1
 34a:	84 60       	ori	r24, 0x04	; 4
 34c:	80 93 b1 00 	sts	0x00B1, r24	;  0x8000b1
 350:	80 91 b0 00 	lds	r24, 0x00B0	;  0x8000b0
 354:	81 60       	ori	r24, 0x01	; 1
 356:	80 93 b0 00 	sts	0x00B0, r24	;  0x8000b0
 35a:	80 91 7a 00 	lds	r24, 0x007A	;  0x80007a
 35e:	84 60       	ori	r24, 0x04	; 4
 360:	80 93 7a 00 	sts	0x007A, r24	;  0x80007a
 364:	80 91 7a 00 	lds	r24, 0x007A	;  0x80007a
 368:	82 60       	ori	r24, 0x02	; 2
 36a:	80 93 7a 00 	sts	0x007A, r24	;  0x80007a
 36e:	80 91 7a 00 	lds	r24, 0x007A	;  0x80007a
 372:	81 60       	ori	r24, 0x01	; 1
 374:	80 93 7a 00 	sts	0x007A, r24	;  0x80007a
 378:	80 91 7a 00 	lds	r24, 0x007A	;  0x80007a
 37c:	80 68       	ori	r24, 0x80	; 128
 37e:	80 93 7a 00 	sts	0x007A, r24	;  0x80007a
 382:	10 92 c1 00 	sts	0x00C1, r1	;  0x8000c1
 386:	c2 e0       	ldi	r28, 0x02	; 2
 388:	8c 2f       	mov	r24, r28
 38a:	0e 94 70 00 	call	0xe0	;  0xe0
 38e:	60 e0       	ldi	r22, 0x00	; 0
 390:	8c 2f       	mov	r24, r28
 392:	0e 94 8f 00 	call	0x11e	;  0x11e
 396:	cf 5f       	subi	r28, 0xFF	; 255
 398:	c3 31       	cpi	r28, 0x13	; 19
 39a:	b1 f7       	brne	.-20     	;  0x388
 39c:	81 e7       	ldi	r24, 0x71	; 113
 39e:	e8 2e       	mov	r14, r24
 3a0:	80 e0       	ldi	r24, 0x00	; 0
 3a2:	f8 2e       	mov	r15, r24
 3a4:	0e ef       	ldi	r16, 0xFE	; 254
 3a6:	10 e0       	ldi	r17, 0x00	; 0
 3a8:	ce ef       	ldi	r28, 0xFE	; 254
 3aa:	90 e0       	ldi	r25, 0x00	; 0
 3ac:	c9 2e       	mov	r12, r25
 3ae:	90 e0       	ldi	r25, 0x00	; 0
 3b0:	d9 2e       	mov	r13, r25
 3b2:	89 e0       	ldi	r24, 0x09	; 9
 3b4:	0e 94 70 00 	call	0xe0	;  0xe0
 3b8:	f7 01       	movw	r30, r14
 3ba:	84 91       	lpm	r24, Z
 3bc:	83 30       	cpi	r24, 0x03	; 3
 3be:	69 f1       	breq	.+90     	;  0x41a
 3c0:	48 f4       	brcc	.+18     	;  0x3d4
 3c2:	81 30       	cpi	r24, 0x01	; 1
 3c4:	b9 f0       	breq	.+46     	;  0x3f4
 3c6:	82 30       	cpi	r24, 0x02	; 2
 3c8:	19 f1       	breq	.+70     	;  0x410
 3ca:	61 e0       	ldi	r22, 0x01	; 1
 3cc:	89 e0       	ldi	r24, 0x09	; 9
 3ce:	0e 94 8f 00 	call	0x11e	;  0x11e
 3d2:	14 c0       	rjmp	.+40     	;  0x3fc
 3d4:	87 30       	cpi	r24, 0x07	; 7
 3d6:	59 f1       	breq	.+86     	;  0x42e
 3d8:	88 30       	cpi	r24, 0x08	; 8
 3da:	89 f1       	breq	.+98     	;  0x43e
 3dc:	84 30       	cpi	r24, 0x04	; 4
 3de:	a9 f7       	brne	.-22     	;  0x3ca
 3e0:	80 91 80 00 	lds	r24, 0x0080	;  0x800080
 3e4:	80 62       	ori	r24, 0x20	; 32
 3e6:	80 93 80 00 	sts	0x0080, r24	;  0x800080
 3ea:	10 93 8b 00 	sts	0x008B, r17	;  0x80008b
 3ee:	00 93 8a 00 	sts	0x008A, r16	;  0x80008a
 3f2:	04 c0       	rjmp	.+8      	;  0x3fc
 3f4:	84 b5       	in	r24, 0x24	; 36
 3f6:	80 68       	ori	r24, 0x80	; 128
 3f8:	84 bd       	out	0x24, r24	; 36
 3fa:	c7 bd       	out	0x27, r28	; 39
 3fc:	0e 94 00 01 	call	0x200	;  0x200
 400:	0e 94 00 01 	call	0x200	;  0x200
 404:	c1 14       	cp	r12, r1
 406:	d1 04       	cpc	r13, r1
 408:	a1 f2       	breq	.-88     	;  0x3b2
 40a:	0e 94 00 00 	call	0	;  0x0
 40e:	d1 cf       	rjmp	.-94     	;  0x3b2
 410:	84 b5       	in	r24, 0x24	; 36
 412:	80 62       	ori	r24, 0x20	; 32
 414:	84 bd       	out	0x24, r24	; 36
 416:	c8 bd       	out	0x28, r28	; 40
 418:	f1 cf       	rjmp	.-30     	;  0x3fc
 41a:	80 91 80 00 	lds	r24, 0x0080	;  0x800080
 41e:	80 68       	ori	r24, 0x80	; 128
 420:	80 93 80 00 	sts	0x0080, r24	;  0x800080
 424:	10 93 89 00 	sts	0x0089, r17	;  0x800089
 428:	00 93 88 00 	sts	0x0088, r16	;  0x800088
 42c:	e7 cf       	rjmp	.-50     	;  0x3fc
 42e:	80 91 b0 00 	lds	r24, 0x00B0	;  0x8000b0
 432:	80 68       	ori	r24, 0x80	; 128
 434:	80 93 b0 00 	sts	0x00B0, r24	;  0x8000b0
 438:	c0 93 b3 00 	sts	0x00B3, r28	;  0x8000b3
 43c:	df cf       	rjmp	.-66     	;  0x3fc
 43e:	80 91 b0 00 	lds	r24, 0x00B0	;  0x8000b0
 442:	80 62       	ori	r24, 0x20	; 32
 444:	80 93 b0 00 	sts	0x00B0, r24	;  0x8000b0
 448:	c0 93 b4 00 	sts	0x00B4, r28	;  0x8000b4
 44c:	d7 cf       	rjmp	.-82     	;  0x3fc
 44e:	f8 94       	cli
 450:	ff cf       	rjmp	.-2      	;  0x450
