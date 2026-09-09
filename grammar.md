**GOAL** -> INSTRUCTION_LIST

**INSTRUCTION_LIST** -> 
	INSTRUCTION INSTRUCTION_LIST |
	E	

**INSTRUCTION** -> 
	var VAR
	if IF |
	while WHILE |
	return RETURN |
	ID ASSIGNMENT

**RETURN** -> EXPR ;

**IF** -> ( EXPR ) BODY ELSE

Данное правило даёт конфиликт, парсер всегда выбирает else BODY (else относится к последнему if)
**ELSE** -> else BODY |
	E

**WHILE** -> ( EXPR ) BODY 

**BODY**-> { INSTRUCTION_LIST } |
	INSTRUCTION

**ASSIGNMENT** -> = EXPR ;

**VAR** -> ID = EXPR VAR1 ;

**VAR1** -> , ID VAR1_1 VAR1 |
	E

**VAR1_1** -> = EXPR |
	E

**EXPR** -> T1 EXPR_1

**EXPR_1** -> or EXPR |
	E

**T1** -> T2 T1_1

**T1_1** -> and T1 |
	E

**T2** -> T3 T2_1

**T2_1** -> < T2 |
	> T2 |
	<= T2 |
	>= T2 |
	== T2 |
	!= T2 |
	E

**T3** -> T4 T3_1

**T3_1** -> + T3 |
	- T3 |
	E

**T4** -> T5 T4_1

**T4_1** -> * T4 |
	/ T4 |
	E

**T5** -> T6 T5_1

**T5_1** -> . T5 |
	E
    
**T6** ->   NUM |
  ID |
  CALL |
  ( CONDITION ) |
  OBJECT

**CALL** -> API . ID . ID ( CALL_1 )

**CALL_1** -> OBJECT ) |
	)

**OBJECT** -> { FIELDS }

**FIELDS** -> STRING_ID : EXPR FIELDS_1 | 
E

**FIELDS_1** -> , FIELDS |
   E