Feature: MortageCalculatorCheck
        Run the mortgage calculator , calculate the results & check the given output
		
		 
 Background:
  Given alteryx running at" http://gallery.alteryx.com/"
  And I am logged in using "deepak.manoharan@accionlabs.com" and "P@ssw0rd"
  And I publish the application "mortage calculator"
  And I check if the application is "Valid"



Scenario Outline: publish and run Mortage Calculator
When I run mortgage calculator with principle <principle> interest <interest> payments <number of payments>
Then I see output <result>
Examples: 
| principle | interest | number of payments | result  |
| 100000    | 0.04     | 36                 | 2779.49 |


  
 