Feature: RegressionBatchTests.feature

This contains the Batch processing tests done as part of the Core Releases(BGR/PO/Aardvark release) regression test suite

@BatchRegression @BatchRegressionUAT @UAT
Scenario Outline: BatchProcessingAndCompareUAT
	Given I Submit Data Files
	| DataFileName1   | DataFileName2   | TestDataAndSamplesFolder   | INDataFolder   |
	| <DataFileName1> | <DataFileName2> | <TestDataAndSamplesFolder> | <INDataFolder> |
	When Run zip is in archive location
	| ArchiveLocation   |
	| <ArchiveLocation> |
	Then Create GToP Samples
	And Copy files to TestFolder
	And Compare Files with Baseline Files
	| BaselineSamplesFolder   |
	| <BaselineSamplesFolder> |

Examples: 
| TestScenario | DataFileName1                    | DataFileName2 | TestDataAndSamplesFolder                                                 | INDataFolder                                                                                     | ArchiveLocation                                                          | BaselineSamplesFolder                                                              |
| UAT_Medibank | MADHOC_2070884759_2021_11_23.zip |               | \\\\sydcfile2\\CTG\\JPConsole\\Users\\Testers\\Automation\\UAT\\Medibank | \\\\MELDVDACMCEN01\\data_transfer\\External_Clients\\Medibank\\IN\\DELPHI\\DRY\\BATCH\\GAML_LIVE | \\\\MELDVDACMCEN01\\ccs_Archive\\Medibank_955MB\\955MBC_DelPHI           | \\\\sydcfile2\\CTG\\JPConsole\\Users\\Testers\\Automation\\UAT\\Baseline\\Medibank |
| UAT_AGL      | AGL_F_CS_08_20211129_211129.zip  |               | \\\\sydcfile2\\CTG\\JPConsole\\Users\\Testers\\Automation\\UAT\\AGL      | \\\\MELDVDACMCEN01\\data_transfer\\External_Clients\\AGL_950AG\\ONBOARDING\\IN\\DRY              | \\\\MELDVDACMCEN01\\ccs_Archive\\AGL_Energy_950AG\\950AGA_AGL_Onboarding | \\\\sydcfile2\\CTG\\JPConsole\\Users\\Testers\\Automation\\UAT\\Baseline\\AGL      |
#| UAT_MeBank   |               |               |                          |              |                  |                       |

@BatchRegression @BatchRegressionHO @HO
Scenario Outline: BatchProcessingAndCompareHO
	Given I Submit Data Files
	| DataFileName1   | DataFileName2   | TestDataAndSamplesFolder   | INDataFolder   |
	| <DataFileName1> | <DataFileName2> | <TestDataAndSamplesFolder> | <INDataFolder> |
	When Run zip is in archive location
	| ArchiveLocation   |
	| <ArchiveLocation> |
	Then Copy files to TestFolder
	And Compare Files with Baseline Files
	| BaselineSamplesFolder   |
	| <BaselineSamplesFolder> |

Examples: 
| TestScenario | DataFileName1                    | DataFileName2 | TestDataAndSamplesFolder                                                 | INDataFolder                                                                                     | ArchiveLocation                                                | BaselineSamplesFolder                                                    |
| HO_Medibank  | MADHOC_2070884759_2021_11_23.zip |               | \\\\sydcfile2\\CTG\\JPConsole\\Users\\Testers\\Automation\\UAT\\Medibank | \\\\MELDVDACMCEN01\\data_transfer\\External_Clients\\Medibank\\IN\\DELPHI\\DRY\\BATCH\\GAML_LIVE | \\\\MELDVDACMCEN01\\ccs_Archive\\Medibank_955MB\\955MBC_DelPHI | \\\\sydcfile2\\CTG\\JPConsole\\Users\\Testers\\Automation\\UAT\\Baseline |
#| HO_Medibank  |               |               |                          |              |                  |                       |
#| HO_AGL       |               |               |                          |              |                  |                       |
#| HO_MeBank     |              |               |                          |              |                  |                       |
#| HO_UnityWater |              |               |                          |			  |                  |                       |


